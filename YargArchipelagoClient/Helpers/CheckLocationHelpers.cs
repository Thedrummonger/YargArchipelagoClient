using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Forms;

namespace YargArchipelagoClient.Helpers
{
    public static class WinFormCheckLocationHelpers
    {
        public static void CheckLocations(ConfigData Config, ConnectionData Connection, IEnumerable<SongLocation> locations, bool SkipConfirmPrompt)
        {
            if (!Config!.ManualMode) return;
            var locationIDs = new HashSet<long>();
            var CheckStateChanged = new HashSet<SongLocation>();

            foreach (var songLocation in locations)
            {

                if (songLocation.FameCheckAvailable(Connection, out var fl))
                {
                    locationIDs.Add(fl);
                    continue;
                }

                List<long> ToCheck = [];
                var buttons = new List<CustomMessageResult>();
                int btnCheckCount = 0;
                if (songLocation.StandardCheckAvailable(Connection, out _))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward1);
                }
                if (songLocation.ExtraCheckAvailable(Connection, out _))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward2);
                }
                if (btnCheckCount > 1)
                    buttons.Add(CustomMessageResult.Both);

                var result = SkipConfirmPrompt ?
                    CustomMessageResult.Both :
                    APSongMessageBox.Show(
                    $"Check Song {songLocation.GetSongDisplayName(Config!, false, false, true)}",
                    songLocation.GetSongDisplayName(Config!, true, true, false),
                    [.. buttons]);

                if (result.In(CustomMessageResult.Reward1, CustomMessageResult.Both) && songLocation.StandardCheckAvailable(Connection, out var sl1))
                    ToCheck.Add(sl1);
                if (result.In(CustomMessageResult.Reward2, CustomMessageResult.Both) && songLocation.ExtraCheckAvailable(Connection, out var el1))
                    ToCheck.Add(el1);
                if (songLocation.FameCheckAvailable([.. Connection.CheckedLocations, .. ToCheck], out var fl2))
                    ToCheck.Add(fl2);

                if (ToCheck.Count > 0) CheckStateChanged.Add(songLocation);
                locationIDs = [.. locationIDs, .. ToCheck];
            }
            Connection.CommitCheckLocations(locationIDs, CheckStateChanged, Config);
        }
    }
}
