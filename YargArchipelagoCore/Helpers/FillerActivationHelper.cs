using TDMUtils;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;
using static YargArchipelagoCommon.CommonData;
using static YargArchipelagoCore.Data.APWorldData;

namespace YargArchipelagoCore.Helpers
{
    public class FillerActivationHelper(ConnectionData connection, ConfigData config, SongLocation song)
    {
        public void LowerReward1Diff(StaticYargAPItem? itemUsed)
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward1Diff = (CommonData.SupportedDifficulty)((int)Req.Reward1Diff - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode && itemUsed is not null)
                config.ApItemsUsed.Add(itemUsed);
            config.SaveConfigFile(connection);
        }
        public void LowerReward2Diff(StaticYargAPItem? itemUsed)
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward2Diff = (CommonData.SupportedDifficulty)((int)Req.Reward2Diff - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode && itemUsed is not null)
                config.ApItemsUsed.Add(itemUsed);
            config.SaveConfigFile(connection);
        }
        public void LowerReward1Req(StaticYargAPItem? itemUsed)
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward1Req = (APWorldData.CompletionReq)((int)Req.Reward1Req - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode && itemUsed is not null)
                config.ApItemsUsed.Add(itemUsed);
            config.SaveConfigFile(connection);
        }
        public void LowerReward2Req(StaticYargAPItem? itemUsed)
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward2Req = (APWorldData.CompletionReq)((int)Req.Reward2Req - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode && itemUsed is not null)
                config.ApItemsUsed.Add(itemUsed);
            config.SaveConfigFile(connection);
        }

        public SongData[] GetValidSongReplacements()
        {
            var ValidForProfile = song.Requirements!.GetAvailableSongs(config.SongData).Values.ToHashSet();
            return [.. ValidForProfile.Where(x => !config.ApLocationData.Values.Any(y => y.SongHash == x.SongChecksum))];
        }
    }
}
