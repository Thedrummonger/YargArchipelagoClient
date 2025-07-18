using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData;
using static YargArchipelagoCommon.CommonData.Networking;

namespace YargArchipelagoPlugin
{
    public class YargPacketClient
    {
        private readonly string serverIP = "127.0.0.1"; // Always connect to localhost.
        private readonly int serverPort = CommonData.Networking.PORT;
        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;
        private ArchipelagoService APHandler;

        public event Action<DeathLinkData> DeathLinkReceived;
        public event Action<ActionItemData> ActionItemReceived;
        public event Action<(string SongHash, string Profile)[]> AvailableSongsReceived;

        public bool IsConnected => client != null && client.Connected;

        public YargPacketClient(ArchipelagoService handler)
        {
            cts = new CancellationTokenSource();
            APHandler = handler;
        }

        public async Task ConnectAsync()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    client = new TcpClient();
                    APHandler.Log("Listening for Yarg Client");
                    await client.ConnectAsync(serverIP, serverPort);
                    stream = client.GetStream();
                    APHandler.Log("YARG client connected to AP Packet Server.");
                    await ReceiveLoopAsync();
                }
                catch (Exception ex)
                {
                    APHandler.Log("YARG client failed to connect: " + ex.Message);
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
                            APHandler.Log("AP Packet Server disconnected.");
                            break;
                        }
                        APHandler.Log("Received from AP Packet Server: ");
                        ParseClientPacket(line);
                    }
                }
                catch (Exception ex)
                {
                    APHandler.Log("Error in YARG client receive loop: " + ex.Message);
                }
            }
        }

        public async Task SendPacketAsync(Networking.YargAPPacket packet)
        {
            if (!IsConnected || stream == null)
                return;
            string json = JsonConvert.SerializeObject(packet, Networking.PacketSerializeSettings) + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                APHandler.Log("Error sending packet from YARG client: " + ex.Message);
            }
        }

        internal void ParseClientPacket(string line)
        {
            YargAPPacket BasePacket;
            try
            {
                BasePacket = JsonConvert.DeserializeObject<YargAPPacket>(line, PacketSerializeSettings);
            }
            catch (Exception e)
            {
                APHandler.Log($"Failed to parse client packet\n{line}\n{e}");
                return;
            }

            if (BasePacket.deathLinkData != null)
                DeathLinkReceived?.Invoke(BasePacket.deathLinkData);

            if (BasePacket.ActionItem != null)
                ActionItemReceived?.Invoke(BasePacket.ActionItem);

            if (BasePacket.AvailableSongs != null)
                AvailableSongsReceived?.Invoke(BasePacket.AvailableSongs);
        }
    }
}
