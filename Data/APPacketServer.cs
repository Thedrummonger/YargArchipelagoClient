using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Archipelago.MultiClient.Net.Helpers;
using YargArchipelagoClient.Helpers;
using ArchipelagoPowerTools.Data;

namespace YargArchipelagoClient.Data
{
    public class APPacketServer
    {
        private TcpListener listener;
        private CancellationTokenSource cts;
        private StreamWriter currentWriter;
        private ConfigData Config;
        private ConnectionData Connection;
        private Action<string> Logger;
        private Action<string> SongMonitor;
        private Action<bool> ConnectionChanged;

        public APPacketServer(ConfigData config, ConnectionData connection, Action<string> logger, Action<string> songMonitor, Action<bool> connectionChanged)
        {
            listener = new TcpListener(IPAddress.Any, CommonData.Networking.PORT);
            cts = new CancellationTokenSource();
            Config = config;
            Connection = connection;
            Logger = logger;
            SongMonitor = songMonitor;
            ConnectionChanged = connectionChanged;
        }

        public async Task StartAsync()
        {
            listener.Start();
            Debug.WriteLine("AP Packet Server started, waiting for YARG client connection...");

            // Only one client is accepted at a time.
            while (!cts.Token.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Logger?.Invoke("YARG game Client connected.");
                ConnectionChanged?.Invoke(true);
                await HandleClientAsync(client, cts.Token);
                ConnectionChanged?.Invoke(false);
            }

            listener.Stop();
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
            Logger?.Invoke("YARG game client disconnected.");
            currentWriter = null;
        }

        private void ParsePacket(string line)
        {
            try
            {
                var Packet = JsonConvert.DeserializeObject<CommonData.Networking.YargDataPacket>(line);
                if (Packet is null) return;
                if (Packet.passInfo is not null)
                    CheckLocationHelpers.CheckLocations(Config, Connection, Packet.passInfo);
                if (Packet.Message is not null)
                    Logger?.Invoke(Packet.Message);
                if (Packet.CurrentlyPlaying is not null)
                    SongMonitor?.Invoke(Packet.CurrentlyPlaying);
            }
            catch { return; }
        }

        public async Task SendPacketAsync(CommonData.Networking.ClientDataPacket packet)
        {
            if (currentWriter != null)
            {
                string json = JsonConvert.SerializeObject(packet) + "\n";
                await currentWriter.WriteAsync(json);
            }
        }
    }
}
