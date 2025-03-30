using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TDMUtils;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCommon;

namespace YargArchipelagoClient.Data
{
    public class APPacketServer
    {
        private TcpListener listener;
        private CancellationTokenSource cts;
        private StreamWriter? currentWriter;
        private ConfigData Config;
        private ConnectionData Connection;

        public event Action<string> LogMessage;
        public event Action<CommonData.SongData> CurrentSongUpdated;
        public event Action<bool> ConnectionChanged;
        public event Action<string> PacketServerClosed;

        public APPacketServer(ConfigData config, ConnectionData connection)
        {
            listener = new TcpListener(IPAddress.Any, CommonData.Networking.PORT);
            cts = new CancellationTokenSource();
            Config = config;
            Connection = connection;
        }

        public async Task StartAsync()
        {
            try
            {
                listener.Start();
                Debug.WriteLine("AP Packet Server started, waiting for YARG client connection...");
            }
            catch 
            {
                PacketServerClosed.Invoke("Failed To Start Listener");
                return; 
            }

            // Only one client is accepted at a time.
            while (!cts.Token.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                LogMessage?.Invoke("YARG game Client connected.");
                await HandleClientAsync(client, cts.Token);
            }

            listener.Stop();
            PacketServerClosed.Invoke("Connection was canceled");
            Debug.WriteLine("AP Packet Server stopped.");
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
            {
                currentWriter = writer;
                ConnectionChanged?.Invoke(true);
                // In this example we assume that the YARG client sends packets that we process.
                while (!token.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null)
                        break;
                    Debug.WriteLine("Received from YARG client: " + line);
                    // Process the incoming packet, for example:
                    ParsePacket(line);
                }
            }
            ConnectionChanged?.Invoke(false);
            LogMessage?.Invoke("YARG game client disconnected.");
            currentWriter = null;
        }

        private void ParsePacket(string line)
        {
            try
            {
                var Packet = JsonConvert.DeserializeObject<CommonData.Networking.YargAPPacket>(line, CommonData.Networking.PacketSerializeSettings);
                if (Packet is null) return;
                if (Packet.passInfo is not null)
                    CheckLocationHelpers.CheckLocations(Config, Connection, Packet.passInfo);
                if (Packet.Message is not null)
                    LogMessage?.Invoke(Packet.Message);
                if (Packet.CurrentlyPlaying is not null)
                    CurrentSongUpdated?.Invoke(Packet.CurrentlyPlaying.song);
            }
            catch
            {
                Debug.WriteLine($"Failed to Parse Packet\n{line.ToFormattedJson()}");
                return;
            }
        }

        public async Task SendPacketAsync(CommonData.Networking.YargAPPacket packet)
        {
            if (currentWriter is null) return;
            string json = JsonConvert.SerializeObject(packet, CommonData.Networking.PacketSerializeSettings) + "\n";
            await currentWriter.WriteAsync(json);
        }
    }
}
