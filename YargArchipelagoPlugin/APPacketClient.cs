using BepInEx.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoPlugin
{
    public class YargPacketClient
    {
        private readonly string serverIP = "127.0.0.1"; // Always connect to localhost.
        private readonly int serverPort = CommonData.Networking.PORT;
        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;

        public bool IsConnected => client != null && client.Connected;

        public YargPacketClient()
        {
            cts = new CancellationTokenSource();
        }

        public async Task ConnectAsync()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    client = new TcpClient();
                    ArchipelagoPlugin.ManualLogSource?.LogInfo("Listening for Yarg Client");
                    await client.ConnectAsync(serverIP, serverPort);
                    stream = client.GetStream();
                    ArchipelagoPlugin.ManualLogSource?.LogInfo("YARG client connected to AP Packet Server.");
                    await ReceiveLoopAsync();
                }
                catch (Exception ex)
                {
                    ArchipelagoPlugin.ManualLogSource?.LogInfo("YARG client failed to connect: " + ex.Message);
                    await Task.Delay(1000);
                }
            }
        }

        private async Task ReceiveLoopAsync()
        {
            if (stream == null)
                return;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line == null)
                        {
                            ArchipelagoPlugin.ManualLogSource?.LogInfo("AP Packet Server disconnected.");
                            break;
                        }
                        ArchipelagoPlugin.ManualLogSource?.LogInfo("Received from AP Packet Server: ");
                        Archipelago.ParseClientPacket(line);
                    }
                }
                catch (Exception ex)
                {
                    ArchipelagoPlugin.ManualLogSource?.LogInfo("Error in YARG client receive loop: " + ex.Message);
                }
            }
        }

        public async Task SendPacketAsync(Networking.YargDataPacket packet)
        {
            if (!IsConnected || stream == null)
                return;
            string json = JsonConvert.SerializeObject(packet) + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                ArchipelagoPlugin.ManualLogSource?.LogInfo("Error sending packet from YARG client: " + ex.Message);
            }
        }
    }
}
