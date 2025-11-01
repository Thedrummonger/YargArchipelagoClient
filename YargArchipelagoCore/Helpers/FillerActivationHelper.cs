using TDMUtils;
using YargArchipelagoCore.Data;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoCore.Helpers
{
    public class FillerActivationHelper(ConnectionData connection, ConfigData config, SongLocation song)
    {
        public void LowerReward1Diff()
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward1Diff = (CommonData.SupportedDifficulty)((int)Req.Reward1Diff - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode)
            {
                config.UsedFiller.SetIfEmpty(APWorldData.StaticItems.LowerDifficulty, 0);
                config.UsedFiller[APWorldData.StaticItems.LowerDifficulty]++;
            }
            config.SaveConfigFile(connection);
        }
        public void LowerReward2Diff()
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward2Diff = (CommonData.SupportedDifficulty)((int)Req.Reward2Diff - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode)
            {
                config.UsedFiller.SetIfEmpty(APWorldData.StaticItems.LowerDifficulty, 0);
                config.UsedFiller[APWorldData.StaticItems.LowerDifficulty]++;
            }
            config.SaveConfigFile(connection);
        }
        public void LowerReward1Req()
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward1Req = (APWorldData.CompletionReq)((int)Req.Reward1Req - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode)
            {
                config.UsedFiller.SetIfEmpty(APWorldData.StaticItems.LowerDifficulty, 0);
                config.UsedFiller[APWorldData.StaticItems.LowerDifficulty]++;
            }
            config.SaveConfigFile(connection);
        }
        public void LowerReward2Req()
        {
            var Req = song.Requirements!.CompletionRequirement.DeepClone();
            Req.Reward2Req = (APWorldData.CompletionReq)((int)Req.Reward2Req - 1);
            song.Requirements!.CompletionRequirement = Req;
            if (!config.CheatMode)
            {
                config.UsedFiller.SetIfEmpty(APWorldData.StaticItems.LowerDifficulty, 0);
                config.UsedFiller[APWorldData.StaticItems.LowerDifficulty]++;
            }
            config.SaveConfigFile(connection);
        }

        public SongData[] GetValidSongReplacements()
        {
            var ValidForProfile = song.Requirements!.GetAvailableSongs(config.SongData).Values.ToHashSet();
            return [.. ValidForProfile.Where(x => !config.ApLocationData.Values.Any(y => y.SongHash == x.SongChecksum))];
        }
    }
}
