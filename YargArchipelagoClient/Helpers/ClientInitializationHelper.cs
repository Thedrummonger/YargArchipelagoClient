using Newtonsoft.Json;
using System.Diagnostics;
using YargArchipelagoClient.Data;

namespace YargArchipelagoClient.Helpers
{
    class ClientInitializationHelper
    {
        public static bool ConnectToServer(out ConnectionData? connection)
        {
            connection = null;
            var CForm = new ConnectionForm();
            var dialog = CForm.ShowDialog();
            if (dialog != DialogResult.OK)
                return false;

            connection = CForm.Connection;
            return CForm.Connection is not null &&
                CForm.Connection.GetSession() is not null &&
                CForm.Connection.GetSession()!.Socket.Connected;
        }

        public static bool GetConfig(ConnectionData Connection, out ConfigData? configData)
        {
            configData = null;
            var SeedDir = ConnectionData.GetSeedPath();
            if (!Directory.Exists(SeedDir))
                Directory.CreateDirectory(SeedDir);

            var ConfigFile = Directory.GetFiles(SeedDir).FirstOrDefault(file => Path.GetFileName(file) == Connection.getSaveFileName());
            if (ConfigFile is not null)
            {
                Debug.WriteLine($"Seed Found {ConfigFile}");
                try { configData = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(ConfigFile)); }
                catch { configData = null; }
            }
            if (configData is null)
            {
                var configForm = new ConfigForm(Connection!);
                var Dialog = configForm.ShowDialog();
                if (Dialog != DialogResult.OK)
                    return false;
                configData = configForm.data!;
            }
            return configData is not null;
        }

        public static void ReadSlotData(ConnectionData Connection, ConfigData config)
        {

            var SlotData = Connection!.GetSession().DataStorage.GetSlotData();

            if (SlotData["fame_points_for_goal"] is Int64 FPSlotDataVal)
                config.FamePointsNeeded = (int)FPSlotDataVal;
            else
                throw new Exception("Could not get Fame Point Goal");

            if (SlotData.TryGetValue("death_link", out var DLO) && DLO is Int64 DLI && DLI > 0)
            {
                config.deathLinkEnabled = true;
                Connection.DeathLinkService?.EnableDeathLink();
            }
        }
    }
}
