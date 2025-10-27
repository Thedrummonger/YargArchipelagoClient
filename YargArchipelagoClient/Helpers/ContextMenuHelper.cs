using System.Text.RegularExpressions;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Forms;
using YargArchipelagoCommon;
using static YargArchipelagoClient.Helpers.WinFormHelpers;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoClient.Helpers
{
    static class DeathLinkTimeout
    {
        public static DateTime lastManualDeathLink = DateTime.MinValue;
        public static int ManualDeathLinkTimeout = 5;
    }
    public class ContextMenuBuilder(MainForm mainForm, SongLocation song)
    {
        ConfigData config = mainForm.Config!;
        ConnectionData connection = mainForm.Connection;
        public static ContextMenuStrip BuildSongMenu(MainForm mainForm, SongLocation song) => new ContextMenuBuilder(mainForm, song).BuildSongMenu();
        public ContextMenuStrip BuildSongMenu()
        {
            int RandomSwapsTotal = connection.ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.SwapRandom, out int rst) ? rst : 0;
            int RandomSwapsUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.SwapRandom, out int rsu) ? rsu : 0;
            int RandomSwapsAvailable = RandomSwapsTotal - RandomSwapsUsed;

            int SwapsTotal = connection.ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.SwapPick, out int st) ? st : 0;
            int SwapsUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.SwapPick, out int su) ? su : 0;
            int SwapsAvailable = SwapsTotal - SwapsUsed;

            int LowerDiffTotal = connection.ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.LowerDifficulty, out int lt) ? lt : 0;
            int LowerDiffUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.LowerDifficulty, out int lu) ? lu : 0;
            int LowerDiffAvailable = LowerDiffTotal - LowerDiffUsed;

            var menu = new ContextMenuStrip();

            var SongData = song.GetSongData(config);
            if (SongData is not null)
            {
                menu.Items.AddItem($"Song: {SongData.Name}");
                menu.Items.AddItem($"Artist: {SongData.Artist}");
                menu.Items.AddItem($"Album: {SongData.Album}");
                menu.Items.AddItem($"Charter: {SongData.Charter}");
            }

            if (config.ManualMode)
            {
                menu.Items.Add(new ToolStripSeparator());
                if (song.HasStandardCheck(out var sl) && !connection.CheckedLocations.Contains(sl))
                    menu.Items.AddItem("Check Reward 1", () => connection.CommitCheckLocations([sl], [song], config));

                if (song.HasExtraCheck(out var el) && !connection.CheckedLocations.Contains(el))
                    menu.Items.AddItem("Check Reward 2", () => connection.CommitCheckLocations([el], [song], config));

                if (song.FameCheckAvailable([.. connection.CheckedLocations], out var fl))
                    menu.Items.AddItem("Get Fame Point", () => connection.CommitCheckLocations([el], [song], config));
            }

            if ((RandomSwapsAvailable > 0 || SwapsAvailable > 0) || LowerDiffAvailable > 0 || config.CheatMode)
            {
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem($"Use Modifier:");
                if (RandomSwapsAvailable > 0 || config.CheatMode)
                    menu.Items.AddItem($"{APWorldData.StaticItems.SwapRandom.GetDescription()}: {RandomSwapsAvailable}", () => SwapSong(true));
                if (SwapsAvailable > 0 || config.CheatMode)
                    menu.Items.AddItem($"{APWorldData.StaticItems.SwapPick.GetDescription()}: {SwapsAvailable}", () => SwapSong(false));
                if (LowerDiffAvailable > 0 || config.CheatMode)
                {
                    menu.Items.AddItem($"Lower Difficulty value: {LowerDiffAvailable}");
                    if (song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Diff > CommonData.SupportedDifficulty.Easy)
                        menu.Items.AddItem($"-Reward 1 Min Difficulty", LowerReward1Diff);
                    if (song.HasExtraCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Diff > CommonData.SupportedDifficulty.Easy)
                        menu.Items.AddItem($"-Reward 2 Min Difficulty", LowerReward2Diff);

                    if (song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Req > APWorldData.CompletionReq.Clear)
                        menu.Items.AddItem($"-Reward 1 Min Score Requirement", LowerReward1Req);
                    if (song.HasExtraCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Req > APWorldData.CompletionReq.Clear)
                        menu.Items.AddItem($"-Reward 2 Min Score Requirement", LowerReward2Req);
                }
            }

            if (config.deathLinkEnabled && config.ManualMode)
            {
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem("Send Song Fail Death Link", () =>
                {
                    if((DateTime.UtcNow - DeathLinkTimeout.lastManualDeathLink) < TimeSpan.FromMinutes(DeathLinkTimeout.ManualDeathLinkTimeout))
                    {
                        MessageBox.Show($"Manual Deathlinks can only be sent every {DeathLinkTimeout.ManualDeathLinkTimeout} minutes");
                        return;
                    }
                    connection.DeathLinkService!.SendDeathLink(new(connection.SlotName, $"{connection.SlotName} failed song {song.GetSongDisplayName(config!)}"));
                    DeathLinkTimeout.lastManualDeathLink = DateTime.UtcNow;
                });
            }

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.AddItem($"Completion Requirements:");
            menu.Items.AddItem($"Instrument [{song.Requirements!.Instrument}]");
            if (song.HasStandardCheck(out _))
            {
                menu.Items.AddItem($"Reward 1");
                menu.Items.AddItem($"-Min Difficulty [{song.Requirements!.CompletionRequirement.Reward1Diff}]");
                menu.Items.AddItem($"-Min Score [{song.Requirements!.CompletionRequirement.Reward1Req.ToString().AddSpacesToCamelCase()}]");
            }
            if (song.HasExtraCheck(out _))
            {
                menu.Items.AddItem($"Reward 2");
                menu.Items.AddItem($"-Min Difficulty [{song.Requirements!.CompletionRequirement.Reward2Diff}]");
                menu.Items.AddItem($"-Min Score [{song.Requirements!.CompletionRequirement.Reward2Req.ToString().AddSpacesToCamelCase()}]");
            }

            return menu;
        }

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

        public void SwapSong(bool Random)
        {
            var SwapCandidates = GetValidSongReplacements();
            var SelectList = ContainerItem.ToContainerList(SwapCandidates.OrderBy(x => x.GetSongDisplayName()), x => x.GetSongDisplayName());
            if (SwapCandidates.Length < 1)
            {
                MessageBox.Show($"No unused songs were available for profile {song.Requirements!.Name}", "No Valid Swap Candidates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string? Target = null;
            if (Random)
                Target = SwapCandidates[connection.GetRNG().Next(SwapCandidates.Length)].SongChecksum;
            else if (ValueSelectForm.ShowDialog(SelectList, $"Choose a replacement for ${song.GetSongDisplayName(config, WithSongNum: true)}") is ContainerItem c && c.Value is SongData r)
                Target = r.SongChecksum;

            if (Target is null) return;

            song.SongHash = Target;
            var ItemUsed = Random ? APWorldData.StaticItems.SwapRandom : APWorldData.StaticItems.SwapPick;
            if (!config.CheatMode)
            {
                config!.UsedFiller.SetIfEmpty(ItemUsed, 0);
                config!.UsedFiller[ItemUsed]++;
            }

            config.SaveConfigFile(connection);
            mainForm.SafeInvoke(mainForm.PrintSongs);
            CheckLocationHelpers.SendAvailableSongUpdate(config, connection);
        }
    }
    public static class ContextMenuHelper
    {
        public static ToolStripItem AddItem(this ToolStripItemCollection items, string name)
        {
            var label = new ToolStripLabel(name);
            items.Add(label);
            return label;
        }

        public static ToolStripItem AddItem(this ToolStripItemCollection items, string name, Action onClick)
        {
            var actionItem = new ToolStripMenuItem(name);
            actionItem.Click += (s, e) => onClick();
            items.Add(actionItem);
            return actionItem;
        }
        public static string AddSpacesToCamelCase(this string text) => Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
    }
}
