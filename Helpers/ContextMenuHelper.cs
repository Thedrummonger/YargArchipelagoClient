using ArchipelagoPowerTools.Helpers;
using System.Text.RegularExpressions;
using TDMUtils;
using YargArchipelagoClient.Data;

namespace YargArchipelagoClient.Helpers
{
    public static class ContextMenuHelper
    {
        public static ContextMenuStrip BuildSongListContextMenu(MainForm mainForm, ListViewItem itemTarget, SongLocation song)
        {
            int RandomSwapsTotal = mainForm.ReceivedFiller.TryGetValue(Constants.StaticItems.SwapRandom.GetDescription(), out int rst) ? rst : 0;
            int RandomSwapsUsed = mainForm.Config!.UsedFiller.TryGetValue(Constants.StaticItems.SwapRandom.GetDescription(), out int rsu) ? rsu : 0;
            int RandomSwapsAvailable = RandomSwapsTotal - RandomSwapsUsed;

            int SwapsTotal = mainForm.ReceivedFiller.TryGetValue(Constants.StaticItems.SwapPick.GetDescription(), out int st) ? st : 0;
            int SwapsUsed = mainForm.Config!.UsedFiller.TryGetValue(Constants.StaticItems.SwapPick.GetDescription(), out int su) ? su : 0;
            int SwapsAvailable = SwapsTotal - SwapsUsed;

            var menu = new ContextMenuStrip();

            if (song.HasStandardCheck(out var sl) && !mainForm.CheckedLocations.Contains(sl))
                menu.Items.AddItem("Check Reward 1", () => mainForm.CheckLocations([sl], [song]));

            if (song.HasExtraCheck(out var el) && !mainForm.CheckedLocations.Contains(el))
                menu.Items.AddItem("Check Reward 2", () => mainForm.CheckLocations([el], [song]));

            if (song.FameCheckAvailable([..mainForm.CheckedLocations], out var fl))
                menu.Items.AddItem("Get Fame Point", () => mainForm.CheckLocations([el], [song]));

            if ((RandomSwapsAvailable > 0 || SwapsAvailable > 0))
            {
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem($"Use Modifier:");
                if (RandomSwapsAvailable > 0)
                    menu.Items.AddItem($"{Constants.StaticItems.SwapRandom.GetDescription()}: {RandomSwapsAvailable}", () => SwapSong(mainForm, song, true));
                if (SwapsAvailable > 0)
                    menu.Items.AddItem($"{Constants.StaticItems.SwapPick.GetDescription()}: {SwapsAvailable}", () => SwapSong(mainForm, song, false));
            }

            if (mainForm.deathLinkEnabled)
            {
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem("Send Song Fail Death Link", () => 
                    mainForm.deathLinkService.SendDeathLink(new(mainForm.Connection.SlotName, 
                    $"Failed {MainForm.GetSongBroadcastDisplayString(song, mainForm.Config)}")));
            }

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.AddItem($"Completion Requirements:");
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

        public static string[] GetValidSongReplacements(ConfigData configData, SongLocation song)
        {
            var ValidForProfile = song.Requirements!.GetAvailableSongs(configData.SongData).Keys.ToHashSet();
            return [.. ValidForProfile.Where(x => !configData.ApLocationData.Values.Any(y => y.MappedSong == x))];
        }

        public static void SwapSong(MainForm mainForm, SongLocation song, bool Random)
        {
            var SwapCandidates = GetValidSongReplacements(mainForm.Config!, song);
            if (SwapCandidates.Length < 1)
            {
                MessageBox.Show($"No unused songs were available for profile {song.Requirements!.Name}", "No Valid Swap Candidates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string? Target = null;
            if (Random)
                Target = SwapCandidates[mainForm.Connection.SeededRNG.Next(SwapCandidates.Length)];
            else if (ValueSelectForm.ShowDialog(SwapCandidates.OrderBy(x => x), $"Choose a replacement for ${song.MappedSong}") is string r)
                Target = r;

            if (Target is null) return;

            song.MappedSong = Target;
            string ItemUsed = Random ? Constants.StaticItems.SwapRandom.GetDescription() : Constants.StaticItems.SwapPick.GetDescription();
            mainForm.Config!.UsedFiller.SetIfEmpty(ItemUsed, 0);
            mainForm.Config!.UsedFiller[ItemUsed]++;

            mainForm.SafeInvoke(mainForm.UpdateConfigFile);
            mainForm.SafeInvoke(mainForm.PrintSongs);
        }

        public static string AddSpacesToCamelCase(this string text) => Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");

        public static ToolStripItem AddItem(this ToolStripItemCollection items, string name, Action onClick)
        {
            var actionItem = new ToolStripMenuItem(name);
            actionItem.Click += (s, e) => onClick();
            items.Add(actionItem);
            return actionItem;
        }

        public static ToolStripItem AddItem(this ToolStripItemCollection items, string name)
        {
            var label = new ToolStripLabel(name);
            items.Add(label);
            return label;
        }
    }
}
