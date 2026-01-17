extern alias TDMAP;

using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using TDMAP::Archipelago.MultiClient.Net;
using TDMAP.Archipelago.MultiClient.Net.MessageLog.Messages;
using TDMAP.Archipelago.MultiClient.Net.MessageLog.Parts;
using TDMAP.Archipelago.MultiClient.Net.Models;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;
using static YargArchipelagoCore.Data.ArchipelagoColorHelper;

namespace YargArchipelagoCore.Helpers
{
    public static partial class ExtraAPFunctionalityHelper
    {
        public const long minEnergyLinkScale = 20000;
        public const long maxEnergyLinkScale = 1000000;

        public const long SwapSongRandomPrice = 17_000_000_000;
        public const long SwapSongPickPrice = 20_000_000_000;
        public const long LowerDifficultyPrice = 15_000_000_000;

        public static string EnergyLinkKey(ArchipelagoSession session) => $"EnergyLink{session.Players.ActivePlayer.Team}";
        public static bool TryPurchaseItem(ConnectionData connection, ConfigData config, APWorldData.StaticItems Type, long Price)
        {
            var CurrentEnergy = GetEnergy(connection, config);
            if (!TryUseEnergy(connection, config, Price))
                return false;
            var CurCount = config.ApItemsPurchased.Where(x => x.Type == Type).Count();
            config.ApItemsPurchased.Add(new(Type, APWorldData.APIDs.IDFromStaticItem[Type], -99, CurCount, "YARGAPSHOP"));
            config.SaveConfigFile(connection);
            return true;
        }

        public static string FormatLargeNumber(long number)
        {
            if (number >= 1_000_000_000_000)
                return (number / 1_000_000_000_000.0).ToString("0.##") + " Trillion";
            if (number >= 1_000_000_000)
                return (number / 1_000_000_000.0).ToString("0.##") + " Billion";
            if (number >= 1_000_000)
                return (number / 1_000_000.0).ToString("0.##") + " Million";
            if (number >= 1_000)
                return (number / 1_000.0).ToString("0.##") + " Thousand";

            return number.ToString("N0");
        }
        public static void SendScoreAsEnergy(ConnectionData connection, ConfigData config, long BaseScore, bool WasLocationChecked)
        {
            if (config.EnergyLinkMode <= CommonData.EnergyLinkType.None) return;
            if (config.EnergyLinkMode == CommonData.EnergyLinkType.CheckSong && !WasLocationChecked) return;
            if (config.EnergyLinkMode == CommonData.EnergyLinkType.OtherSong && WasLocationChecked) return;

            var Session = connection.GetSession();
            Session.DataStorage[EnergyLinkKey(Session)].Initialize(0);
            Session.DataStorage[EnergyLinkKey(Session)] += ScaleEnergyValue(connection, config, BaseScore);
        }

        public static long ScaleEnergyValue(ConnectionData connection, ConfigData config, long baseAmount)
        {
            int AmountOfLocationsTotal = connection.GetSession().Locations.AllLocations.Count;
            int AmountOfLocationsChecked = connection.GetSession().Locations.AllLocationsChecked.Count;
            double completionPercentage = AmountOfLocationsChecked / AmountOfLocationsTotal;
            double scale = minEnergyLinkScale + (completionPercentage * (maxEnergyLinkScale - minEnergyLinkScale));
            long Energy = (long)(baseAmount * scale);
            return Energy;
        }

        public static long GetEnergy(ConnectionData connection, ConfigData config)
        {
            if (config.EnergyLinkMode <= CommonData.EnergyLinkType.None) return 0;
            var Session = connection.GetSession();
            Session.DataStorage[EnergyLinkKey(Session)].Initialize(0);
            return Session.DataStorage[EnergyLinkKey(Session)];
        }

        public static bool TryUseEnergy(ConnectionData connection, ConfigData config, long Amount)
        {
            if (config.EnergyLinkMode <= CommonData.EnergyLinkType.None) return false;
            var Session = connection.GetSession();
            Session.DataStorage[EnergyLinkKey(Session)].Initialize(0);
            if (Session.DataStorage[EnergyLinkKey(Session)] >= Amount)
            {
                Session.DataStorage[EnergyLinkKey(Session)] -= Amount;
                return true;
            }
            return false;

        }
        private static PropertyInfo? _textProperty;
        public static void FormatYargItemNames(this LogMessage message, ConfigData config)
        {
            if (message is ItemSendLogMessage itemSend)
            {
                if (itemSend.IsReceiverTheActivePlayer)
                {
                    foreach (var part in message.Parts.Where(x => x.Type == MessagePartType.Item))
                    {
                        try
                        {
                            string OriginalName = part.Text.Trim();
                            var SongHash = config.ApLocationData.Values.FirstOrDefault(x => OriginalName == $"Song {x.SongNumber}");
                            if (SongHash is null) continue;
                            _textProperty ??= typeof(MessagePart).GetProperty("Text");
                            _textProperty?.SetValue(part, $"{OriginalName}: {SongHash.GetSongDisplayName(config)}");
                        }
                        catch (Exception ex) { Debug.WriteLine($"Failed to update yarg song message\n{ex.Message}\n{message}"); }
                    }
                }
                if (itemSend.IsSenderTheActivePlayer)
                {
                    foreach(var part in message.Parts.Where(x => x.Type == MessagePartType.Location))
                    {
                        try
                        {
                            if (!IsSongLocation(part.Text, out var Number, out var Reward))
                                continue;
                            var SongHash = config.ApLocationData.Values.FirstOrDefault(x => Number == $"Song {x.SongNumber}");
                            if (SongHash is null) continue;
                            _textProperty ??= typeof(MessagePart).GetProperty("Text");
                            _textProperty?.SetValue(part, $"{Number}: {SongHash.GetSongDisplayName(config)} {Reward}");
                        }
                        catch (Exception ex) { Debug.WriteLine($"Failed to update yarg song message\n{ex.Message}\n{message}"); }
                    }
                }
            }
        }

        [GeneratedRegex(@"(Song \d+) (Reward \d+)")]
        private static partial Regex SongLocationRegex();

        public static bool IsSongLocation(string name, out string Song, out string Reward)
        {
            Song = string.Empty;
            Reward = string.Empty;

            Match match = SongLocationRegex().Match(name);

            if (match.Success)
            {
                Song = match.Groups[1].Value;
                Reward = match.Groups[2].Value;
                return true;
            }

            return false;
        }
    }
}
