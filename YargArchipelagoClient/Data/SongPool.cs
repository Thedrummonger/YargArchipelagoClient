using YargArchipelagoCommon;

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

        public Dictionary<string, CommonData.SongData> GetAvailableSongs(Dictionary<string, CommonData.SongData> songData, Dictionary<string, List<string>> alreadyAssigned) =>
            GetAvailableSongs(songData).Values
                .Where(j => !alreadyAssigned.TryGetValue(Name, out var value) || !value.Contains(j.SongChecksum))
                .ToDictionary(j => j.SongChecksum);

        public bool MetStandard(CommonData.SongPassInfo passInfo, out bool DeathLink) =>
            MetReq(passInfo, out DeathLink, CompletionRequirement.Reward1Req, CompletionRequirement.Reward1Diff);
        public bool MetExtra(CommonData.SongPassInfo passInfo, out bool DeathLink) =>
            MetReq(passInfo, out DeathLink, CompletionRequirement.Reward2Req, CompletionRequirement.Reward2Diff);

        private bool MetReq(CommonData.SongPassInfo passInfo, out bool DeathLink, APWorldData.CompletionReq req, CommonData.SupportedDifficulty diff)
        {
            DeathLink = false;
            var ValidParticipants = passInfo.participants.Where(x => x.instrument == Instrument && x.Difficulty >= diff);
            if (!ValidParticipants.Any())
                return false;
            bool RequirementMet = false;
            foreach (var player in ValidParticipants)
            {
                if (req == APWorldData.CompletionReq.FullCombo && !player.FC) continue;
                if (req == APWorldData.CompletionReq.GoldStar && !player.WasGoldStar) continue;
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
        public APWorldData.CompletionReq Reward1Req = APWorldData.CompletionReq.Clear;
        public APWorldData.CompletionReq Reward2Req = APWorldData.CompletionReq.ThreeStar;
        public CommonData.SupportedDifficulty Reward1Diff = CommonData.SupportedDifficulty.Expert;
        public CommonData.SupportedDifficulty Reward2Diff = CommonData.SupportedDifficulty.Expert;
    }
}
