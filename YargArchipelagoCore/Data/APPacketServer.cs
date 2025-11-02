using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using TDMUtils;
using YargArchipelagoCore.Helpers;
using YargArchipelagoCommon;

namespace YargArchipelagoCore.Data
{
    public class APPipeServer
    {
        private readonly CancellationTokenSource cts = new();
        private StreamWriter? currentWriter;
        private readonly ConfigData Config;
        private readonly ConnectionData Connection;

        public event Action<string>? LogMessage;
        public event Action? CurrentSongUpdated;
        public event Action? ConnectionChanged;
        public event Action<string>? PacketServerClosed;

        public bool IsConnected => currentWriter is not null;

        public APPipeServer(ConfigData config, ConnectionData connection)
        {
            Config = config;
            Connection = connection;
        }

        public async Task StartAsync()
        {
            try
            {
                Debug.WriteLine("AP Pipe Server started, waiting for YARG client connection...");
                while (!cts.Token.IsCancellationRequested)
                {
                    using var server = new NamedPipeServerStream(
                        CommonData.Networking.PipeName,
                        PipeDirection.InOut,
                        maxNumberOfServerInstances: 1,
                        transmissionMode: PipeTransmissionMode.Byte,
                        options: PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(cts.Token);
                    LogMessage?.Invoke("YARG game Client connected.");

                    try { await HandleClientAsync(server, cts.Token); }
                    finally
                    {
                        if (server.IsConnected) server.Disconnect();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
            catch
            {
                PacketServerClosed?.Invoke("Failed To Start Pipe Server");
                return;
            }

            PacketServerClosed?.Invoke("Connection was canceled");
            Debug.WriteLine("AP Pipe Server stopped.");
        }

        private async Task HandleClientAsync(NamedPipeServerStream server, CancellationToken token)
        {
            using var reader = new StreamReader(server, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
            using var writer = new StreamWriter(server, Encoding.UTF8, bufferSize: 8192, leaveOpen: true) { AutoFlush = true };

            currentWriter = writer;
            ConnectionChanged?.Invoke();

            SendClientStatusPacket();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (line is null) break;
                    Debug.WriteLine("Received from YARG client: " + line);
                    ParsePacket(line);
                }
            }
            finally
            {
                ConnectionChanged?.Invoke();
                LogMessage?.Invoke("YARG game client disconnected.");
                currentWriter = null;
            }
        }

        private void ParsePacket(string line)
        {
            try
            {
                var packet = JsonConvert.DeserializeObject<CommonData.Networking.YargAPPacket>(line, CommonData.Networking.PacketSerializeSettings);
                if (packet is null) return;

                if (packet.SongCompletedInfo is not null)
                {
                    Debug.WriteLine($"Song Passed: {packet.SongCompletedInfo.SongPassed}");
                    if (packet.SongCompletedInfo.SongPassed)
                        CheckLocationHelpers.CheckLocations(Config, Connection, packet.SongCompletedInfo);
                    else if (Config.deathLinkEnabled)
                        Connection.DeathLinkService!.SendDeathLink(new(Connection.SlotName, $"{Connection.SlotName} failed song {packet.SongCompletedInfo.songData.GetSongDisplayName(true, true)}"));
                }

                if (packet.Message is not null)
                    LogMessage?.Invoke(packet.Message);

                if (packet.CurrentlyPlaying is not null)
                {
                    Connection.SetCurrentlyPlaying(packet.CurrentlyPlaying.song);
                    CurrentSongUpdated?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to Parse Packet\n{line.ToFormattedJson()}\n{e}");
            }
        }

        public async Task SendPacketAsync(CommonData.Networking.YargAPPacket packet)
        {
            if (currentWriter is null) return;
            var json = JsonConvert.SerializeObject(packet, CommonData.Networking.PacketSerializeSettings) + "\n";
            await currentWriter.WriteAsync(json);
        }

        public void SendClientStatusPacket()
        {
            if (Connection is null || Config is null) return;
            _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket
            {
                AvailableSongs = [.. Config.GetAllSongLocations().Where(x =>
                    x.SongHash is not null &&
                    x.Requirements is not null &&
                    x.SongAvailableToPlay(Connection, Config)).Select(x => (x.SongHash!, x.Requirements!.Name))]
            });
        }

        public void Stop() => cts.Cancel();
    }
}
