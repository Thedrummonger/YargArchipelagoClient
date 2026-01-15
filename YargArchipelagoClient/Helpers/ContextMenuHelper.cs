using System.Diagnostics;
using System.Text;
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
            HashSet<APWorldData.StaticYargAPItem> AvailableItems = [.. connection!.ApItemsRecieved, .. config!.ApItemsPurchased];

            var AllRandomSwap = AvailableItems.Where(x => !config.ApItemsUsed.Contains(x) && x.Type == APWorldData.StaticItems.SwapRandom);
            var UsableRandomSwap = AllRandomSwap.FirstOrDefault();

            var AllPickSwap = AvailableItems.Where(x => !config.ApItemsUsed.Contains(x) && x.Type == APWorldData.StaticItems.SwapPick);
            var UsablePickSwap = AllPickSwap.FirstOrDefault();

            var AllLowerDiff = AvailableItems.Where(x => !config.ApItemsUsed.Contains(x) && x.Type == APWorldData.StaticItems.LowerDifficulty);
            var UsableLowerDiff = AllLowerDiff.FirstOrDefault();

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
                    menu.Items.AddItem($"Use {APWorldData.StaticItems.LowerDifficulty.GetDescription()}: {AllLowerDiff.Count()}");
                    if (song.CanLowerReq1())
                        menu.Items.AddItem($"-Reward 1 Min Score Requirement {song.GetLowerReq1Tag()}", () => fillerActivationHelper.LowerReward1Req(UsableLowerDiff));
                    if (song.CanLowerReq2())
                        menu.Items.AddItem($"-Reward 2 Min Score Requirement {song.GetLowerReq2Tag()}", () => fillerActivationHelper.LowerReward2Req(UsableLowerDiff));
                    if (song.CanLowerDiff1())
                        menu.Items.AddItem($"-Reward 1 Min Difficulty {song.GetLowerDiff1Tag()}", () => fillerActivationHelper.LowerReward1Diff(UsableLowerDiff));
                    if (song.CanLowerDiff2())
                        menu.Items.AddItem($"-Reward 2 Min Difficulty {song.GetLowerDiff2Tag()}", () => fillerActivationHelper.LowerReward2Diff(UsableLowerDiff));

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

            if (song.SongItemReceived(connection, config, out var data))
            {
                var Player = connection.GetSession().Players.GetPlayerInfo(data.SendingPlayerSlot);
                var Location = connection.GetSession().Locations.GetLocationNameFromId(data.SendingPlayerLocation, Player.Game);
                menu.Items.Add(new ToolStripSeparator());
                StringBuilder sb = new();
                sb.Append($"Recieved from {Player.Name}");
                if (Player != 0)
                {
                    sb.Append($" Playing {data.SendingPlayerGame}");
                    if (!String.IsNullOrWhiteSpace(Location))
                        sb.Append(" at:");
                }
                menu.Items.AddItem(sb.ToString());
                if (Player > 0 && !String.IsNullOrWhiteSpace(Location))
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
