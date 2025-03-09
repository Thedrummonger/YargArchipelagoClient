
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

namespace YargArchipelagoClient.Data
{
    public class SongPool(string name, CommonData.SupportedInstrument instrument)
    {
        public string Name = name;
        public CommonData.SupportedInstrument Instrument = instrument;
        public int AmountInPool = 0;
        public int MinDifficulty = 3;
        public int MaxDifficulty = 6;
        public CompletionRequirement CompletionRequirement = new();
        public override string ToString()
        {
            return Name;
        }

        public Dictionary<string, CommonData.SongData> GetAvailableSongs(Dictionary<string, CommonData.SongData> SongData)
        {
            var Data = SongData.Where(x =>
                x.Value.TryGetDifficulty(Instrument, out var difficulty) &&
                difficulty >= MinDifficulty &&
                difficulty <= MaxDifficulty);
            return Data.ToDictionary(x => x.Key, x => x.Value);
        }

        public bool MetStandard(CommonData.SongPassInfo passInfo, out bool DeathLink) =>
            MetReq(passInfo, out DeathLink, CompletionRequirement.Reward1Req, CompletionRequirement.Reward1Diff, Instrument);
        public bool MetExtra(CommonData.SongPassInfo passInfo, out bool DeathLink) =>
            MetReq(passInfo, out DeathLink, CompletionRequirement.Reward2Req, CompletionRequirement.Reward1Diff, Instrument);

        private static bool MetReq(CommonData.SongPassInfo passInfo, out bool DeathLink, CommonData.CompletionReq req, CommonData.SupportedDifficulty diff, CommonData.SupportedInstrument instrument)
        {
            DeathLink = false;
            var ValidParticipants = passInfo.participants.Where(x => x.instrument == instrument);
            if (!ValidParticipants.Any()) return false;
            var HadProperDifficulty = ValidParticipants.Where(x => x.Difficulty >= diff);
            if (!HadProperDifficulty.Any()) return false;
            bool RequirementMet = false;
            foreach (var player in HadProperDifficulty)
            {
                if (req == CommonData.CompletionReq.FullCombo && !player.FC) continue;
                if (req == CommonData.CompletionReq.GoldStar && !player.WasGoldStar) continue;
                if (player.Stars < (int)req) continue;
                RequirementMet = true;
                break;
            }
            DeathLink = !RequirementMet;
            return RequirementMet;
        }
    }

    public class CompletionRequirement()
    {
        public CommonData.CompletionReq Reward1Req = CommonData.CompletionReq.Clear;
        public CommonData.CompletionReq Reward2Req = CommonData.CompletionReq.ThreeStar;
        public CommonData.SupportedDifficulty Reward1Diff = CommonData.SupportedDifficulty.Expert;
        public CommonData.SupportedDifficulty Reward2Diff = CommonData.SupportedDifficulty.Expert;
    }
}
