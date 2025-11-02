using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using TDMUtils;
using YargArchipelagoCore.Helpers;
using YargArchipelagoCommon;
using System.Net.Sockets;
using System.Net;

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
        public event Action<string>? PacketServerError;

        public bool IsConnected => currentWriter is not null;

        public APPipeServer(ConfigData config, ConnectionData connection)
        {
            Config = config;
            Connection = connection;
        }

        public async Task StartAsync()
        {
            if (Config.CurrentUserConfig!.UsePipe)
                await StartAsyncPipe();
            else
                await StartAsyncPacket();
        }

        private async Task StartAsyncPipe()
        {
            try
            {
                Debug.WriteLine("AP Pipe Server started, waiting for YARG client connection...");
                while (!cts.Token.IsCancellationRequested)
                {
                    using var server = new NamedPipeServerStream(
                        Config.CurrentUserConfig!.PipeName,
                        PipeDirection.InOut,
                        maxNumberOfServerInstances: 1,
                        transmissionMode: PipeTransmissionMode.Byte,
                        options: PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(cts.Token);

                    try { await HandleClientAsync(server, cts.Token); } 
                    finally
                    {
                        currentWriter = null;
                        if (server.IsConnected) server.Disconnect();
                        Debug.WriteLine($"YARG game client disconnected. {server.IsConnected}");
                        ConnectionChanged?.Invoke();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Connection was canceled");
            }
            catch
            {
                PacketServerError?.Invoke("Failed To Start Pipe Server");
            }

            Debug.WriteLine("AP Pipe Server stopped.");
        }

        private async Task StartAsyncPacket()
        {
            var listener = new TcpListener(IPAddress.Parse(Config.CurrentUserConfig!.HOST), Config.CurrentUserConfig!.PORT);
            try
            {
                Debug.WriteLine("AP Packet Server started, waiting for YARG client connection...");
                while (!cts.Token.IsCancellationRequested)
                {
                    listener.Start();
                    using var client = await listener.AcceptTcpClientAsync(cts.Token);
                    listener.Stop();
                    var stream = client.GetStream();

                    try { await HandleClientAsync(stream, cts.Token); }
                    finally
                    {
                        currentWriter = null;
                        client.Close();
                        Debug.WriteLine($"YARG game client disconnected. {client.Connected}");
                        ConnectionChanged?.Invoke();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Connection was canceled");
            }
            catch
            {
                PacketServerError?.Invoke("Failed To Start Packet Server");
            }
            try { listener.Stop(); } catch { }

            Debug.WriteLine("AP Packet Server stopped.");
        }
        private async Task HandleClientAsync(Stream server, CancellationToken token)
        {
            using var reader = new StreamReader(server, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
            using var writer = new StreamWriter(server, Encoding.UTF8, bufferSize: 8192, leaveOpen: true) { AutoFlush = true, NewLine = "\n" };
            currentWriter = writer;
            Debug.WriteLine($"YARG game Client connected. {IsConnected()}");
            ConnectionChanged?.Invoke();
            SendClientStatusPacket();
            while (!token.IsCancellationRequested && IsConnected())
            {
                var line = await reader.ReadLineAsync(token).ConfigureAwait(false);
                if (line is null) break;
                Debug.WriteLine("Received from YARG client: " + line);
                ParsePacket(line);
            }

            bool IsConnected()
            {
                if (server is NetworkStream n) return n?.Socket.Connected ?? false;
                else if (server is NamedPipeServerStream p) return p.IsConnected;
                return false;
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
            _ = SendPacketAsync(new CommonData.Networking.YargAPPacket
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
