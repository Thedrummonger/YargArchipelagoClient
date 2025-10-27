using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;

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

        public System.Timers.Timer timer = new System.Timers.Timer(200);

        public event Action<string>? APServerClosed;
        public bool ShouldUpdate = true; //Start true so we do an update when it initializes
        public event Action ConstantCallback;
        public event Action OnUpdateCallback;

        private void SyncTimerTick(object? sender, ElapsedEventArgs e)
        {
            if (connection is null || config is null)
                return;
            if (!connection.GetSession().Socket.Connected)
                APServerClosed?.Invoke("AP server connection lost");
            ConstantCallback?.Invoke();
            if (!ShouldUpdate) return;
            ShouldUpdate = false;
            connection.UpdateCheckedLocations();
            connection.UpdateReceivedItems();
            TrapFillerHelper.SendPendingTrapOrFiller(connection, config);
            connection.GetPacketServer().SendClientStatusPacket();
            OnUpdateCallback?.Invoke();

            //PrintSongs();
            //fame0ToolStripMenuItem.Text = $"Fame: {Connection.GetCurrentFame()} / {config.FamePointsNeeded}";
        }
    }
}
