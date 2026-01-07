using System.Diagnostics;
using System.Text.RegularExpressions;
using TDMUtils;
using YargArchipelagoClient.Forms;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;
using YargArchipelagoCore.Helpers;
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
        FillerActivationHelper fillerActivationHelper = new(mainForm.Connection, mainForm.Config, song);
        public static ContextMenuStrip BuildSongMenu(MainForm mainForm, SongLocation song) => new ContextMenuBuilder(mainForm, song).BuildSongMenu();
        public ContextMenuStrip BuildSongMenu()
        {
            var AllRandomSwap = connection!.ApItemsRecieved.Where(x => !config.ApItemsUsed.Contains(x) && x.Type == APWorldData.StaticItems.SwapRandom);
            var UsableRandomSwap = AllRandomSwap.FirstOrDefault();

            var AllPickSwap = connection!.ApItemsRecieved.Where(x => !config.ApItemsUsed.Contains(x) && x.Type == APWorldData.StaticItems.SwapPick);
            var UsablePickSwap = AllPickSwap.FirstOrDefault();

            var AllLowerDiff = connection!.ApItemsRecieved.Where(x => !config.ApItemsUsed.Contains(x) && x.Type == APWorldData.StaticItems.LowerDifficulty);
            var UsableLowerDiff = AllRandomSwap.FirstOrDefault();

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

            if ((UsableRandomSwap is not null || UsablePickSwap is not null) || UsableLowerDiff is not null || config.CheatMode)
            {
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem($"Use Modifier:");
                if (UsableRandomSwap is not null || config.CheatMode)
                    menu.Items.AddItem($"{APWorldData.StaticItems.SwapRandom.GetDescription()}: {AllRandomSwap.Count()}", () => SwapSong(true, UsableRandomSwap));
                if (UsablePickSwap is not null || config.CheatMode)
                    menu.Items.AddItem($"{APWorldData.StaticItems.SwapPick.GetDescription()}: {AllPickSwap.Count()}", () => SwapSong(false, UsablePickSwap));
                if (UsableLowerDiff is not null || config.CheatMode)
                {
                    menu.Items.AddItem($"Lower Difficulty value: {AllLowerDiff.Count()}");
                    if (song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Diff > CommonData.SupportedDifficulty.Easy)
                        menu.Items.AddItem($"-Reward 1 Min Difficulty", () => fillerActivationHelper.LowerReward1Diff(UsableLowerDiff));
                    if (song.HasExtraCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Diff > CommonData.SupportedDifficulty.Easy)
                        menu.Items.AddItem($"-Reward 2 Min Difficulty", () => fillerActivationHelper.LowerReward2Diff(UsableLowerDiff));

                    if (song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Req > APWorldData.CompletionReq.Clear)
                        menu.Items.AddItem($"-Reward 1 Min Score Requirement", () => fillerActivationHelper.LowerReward1Req(UsableLowerDiff));
                    if (song.HasExtraCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Req > APWorldData.CompletionReq.Clear)
                        menu.Items.AddItem($"-Reward 2 Min Score Requirement", () => fillerActivationHelper.LowerReward2Req(UsableLowerDiff));
                }
            }

            if (config.DeathLinkMode > DeathLinkType.None && config.ManualMode)
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

            if (song.SongItemReceived(connection, out var data))
            {
                var Player = connection.GetSession().Players.GetPlayerInfo(data.SendingPlayerSlot);
                var Location = connection.GetSession().Locations.GetLocationNameFromId(data.SendingPlayerSlot, Player.Game);
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.AddItem($"From {Player.Name} Playing {data.SendingPlayerGame} at");
                menu.Items.AddItem(Location);
            }

            return menu;
        }

        public void SwapSong(bool Random, APWorldData.StaticYargAPItem? UsableSwapItem)
        {
            var SwapCandidates = fillerActivationHelper.GetValidSongReplacements();
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
            if (!config.CheatMode && UsableSwapItem is not null)
                config.ApItemsUsed.Add(UsableSwapItem);

            config.SaveConfigFile(connection);
            mainForm.SafeInvoke(mainForm.PrintSongs);
            connection.GetPacketServer().SendClientStatusPacket();

            Debug.WriteLine(connection.ApItemsRecieved.ToFormattedJson());
            Debug.WriteLine(config.ApItemsUsed.ToFormattedJson());
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
