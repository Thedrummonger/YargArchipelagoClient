using ArchipelagoPowerTools.Data;
using ArchipelagoPowerTools.Helpers;
using System.Text.RegularExpressions;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Forms;

namespace YargArchipelagoClient.Helpers
{
    public static class ContextMenuHelper
    {
        public static ContextMenuStrip BuildSongListContextMenu(MainForm mainForm, SongLocation song)
        {
            ConfigData config = mainForm.Config!;
            ConnectionData connection = mainForm.Connection;
            int RandomSwapsTotal = connection.ReceivedFiller.TryGetValue(APWorldData.StaticItems.SwapRandom, out int rst) ? rst : 0;
            int RandomSwapsUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.SwapRandom, out int rsu) ? rsu : 0;
            int RandomSwapsAvailable = RandomSwapsTotal - RandomSwapsUsed;

            int SwapsTotal = connection.ReceivedFiller.TryGetValue(APWorldData.StaticItems.SwapPick, out int st) ? st : 0;
            int SwapsUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.SwapPick, out int su) ? su : 0;
            int SwapsAvailable = SwapsTotal - SwapsUsed;

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

            if ((RandomSwapsAvailable > 0 || SwapsAvailable > 0))
            {
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem($"Use Modifier:");
                if (RandomSwapsAvailable > 0)
                    menu.Items.AddItem($"{APWorldData.StaticItems.SwapRandom.GetDescription()}: {RandomSwapsAvailable}", () => SwapSong(mainForm, song, true));
                if (SwapsAvailable > 0)
                    menu.Items.AddItem($"{APWorldData.StaticItems.SwapPick.GetDescription()}: {SwapsAvailable}", () => SwapSong(mainForm, song, false));
            }

            if (config.deathLinkEnabled)
            {
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem("Send Song Fail Death Link", () =>
                    connection.DeathLinkService.SendDeathLink(new(connection.SlotName,
                    $"Failed {song.GetSongDisplayName(config!)}")));
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

        public static string[] GetValidSongReplacements(ConfigData configData, SongLocation song)
        {
            var ValidForProfile = song.Requirements!.GetAvailableSongs(configData.SongData).Keys.ToHashSet();
            return [.. ValidForProfile.Where(x => !configData.ApLocationData.Values.Any(y => y.SongHash == x))];
        }

        public static void SwapSong(MainForm main, SongLocation song, bool Random)
        {
            var SwapCandidates = GetValidSongReplacements(main.Config!, song);
            if (SwapCandidates.Length < 1)
            {
                MessageBox.Show($"No unused songs were available for profile {song.Requirements!.Name}", "No Valid Swap Candidates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string? Target = null;
            if (Random)
                Target = SwapCandidates[main.Connection.GetRNG().Next(SwapCandidates.Length)];
            else if (ValueSelectForm.ShowDialog(SwapCandidates.OrderBy(x => x), $"Choose a replacement for ${song.SongHash}") is string r)
                Target = r;

            if (Target is null) return;

            song.SongHash = Target;
            var ItemUsed = Random ? APWorldData.StaticItems.SwapRandom : APWorldData.StaticItems.SwapPick;
            main.Config!.UsedFiller.SetIfEmpty(ItemUsed, 0);
            main.Config!.UsedFiller[ItemUsed]++;

            main.SafeInvoke(main.UpdateConfigFile);
            main.SafeInvoke(main.PrintSongs);
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
