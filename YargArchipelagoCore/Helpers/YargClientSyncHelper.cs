using System.Timers;
using YargArchipelagoCore.Data;

namespace YargArchipelagoCore.Helpers
{
    public class YargClientSyncHelper
    {
        private ConnectionData connection;
        private ConfigData config;
        public YargClientSyncHelper(ConnectionData Connection, ConfigData Config)
        {
            connection = Connection;
            config = Config;
            timer.Elapsed += SyncTimerTick;
        }

        private System.Timers.Timer timer = new System.Timers.Timer(200);

        public event Action<string>? APServerClosed;
        public bool ShouldUpdate = true; //Start true so we do an update when it initializes
        private bool TrapFillerInQueue = false;
        public event Action ConstantCallback;
        public event Action OnUpdateCallback;

        public void StartTimer()
        {
            SyncTimerTick(this, null);
            timer.Start();
        }
        public void StopTimer()
        {
            timer.Stop();
        }

        public void SyncTimerTick(object? sender, ElapsedEventArgs e)
        {
            if (connection is null || config is null)
                return;
            if (!connection.GetSession().Socket.Connected)
                APServerClosed?.Invoke("AP server connection lost");
            if (TrapFillerInQueue)
                TrapFillerInQueue = !TrapFillerHelper.SendPendingTrapOrFiller(connection, config);
            ConstantCallback?.Invoke();
            if (!ShouldUpdate) return;
            ShouldUpdate = false;
            connection.UpdateCheckedLocations();
            connection.UpdateReceivedItems(config);
            TrapFillerInQueue = !TrapFillerHelper.SendPendingTrapOrFiller(connection, config);
            connection.GetPacketServer().SendClientStatusPacket();
            OnUpdateCallback?.Invoke();

            //PrintSongs();
            //fame0ToolStripMenuItem.Text = $"Fame: {Connection.GetCurrentFame()} / {config.FamePointsNeeded}";
        }
    }
}
