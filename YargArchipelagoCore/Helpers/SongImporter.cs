using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoCommon;
using YargArchipelagoCore.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;

namespace YargArchipelagoClient.Helpers
{
    public static class SongImporter
    {
        public static bool TryReadSongs(out Dictionary<string, CommonData.SongData> data)
        {
            string Error = "Your available song data was missing or corrupt. Please run the AP build of YARG at least once before launching the client.\n\n" +
                "You may also need to point YARG a valid song path and run a scan for any newly added songs.\nThis can be found in Settings -> Songs in YARG.";
            data = [];

            string? DataPath = null;
            if (File.Exists("SongExport.json"))
                DataPath = "SongExport.json";
            else if (File.Exists(CommonData.SongExportFile))
                DataPath = CommonData.SongExportFile;

            if (DataPath is null)
            {
                MessageBox.Show(Error, "Song Cache Missing");
                return false;
            }
            try
            {
                CommonData.SongData[]? songData = JsonConvert.DeserializeObject<CommonData.SongData[]>(File.ReadAllText(DataPath));
                if (songData is null || songData.Length == 0)
                {
                    MessageBox.Show(Error, "Song Cache Corrupt");
                    return false;
                }
                foreach (var d in songData)
                    data.Add(d.SongChecksum, d);
            }
            catch
            {
                MessageBox.Show(Error, "Song Cache Corrupt");
                return false;
            }
            return true;
        }

        public static void RescanSongs(ConfigData config, ConnectionData connection)
        {
            int CurrentCount = config.SongData.Count;
            if (!TryReadSongs(out var data))
                return;

            HashSet<SongLocation> UncheckedLocations = [.. config.ApLocationData.Values.Where(x => x.HasUncheckedLocations(connection))];
            HashSet<SongLocation> CheckedLocations = [.. config.ApLocationData.Values.Where(x => !x.HasUncheckedLocations(connection))];
            HashSet<SongLocation> InvalidUnchecked = [.. UncheckedLocations.Where(x => !data.ContainsKey(x.SongHash!))];
            HashSet<SongLocation> ValidUnchecked = [.. UncheckedLocations.Where(x => data.ContainsKey(x.SongHash!))];
            HashSet<SongLocation> InvalidChecked = [.. CheckedLocations.Where(x => !data.ContainsKey(x.SongHash!) && x.SongHash != string.Empty)];
            HashSet<SongLocation> ValidChecked = [.. CheckedLocations.Where(x => data.ContainsKey(x.SongHash!))];

            if (InvalidChecked.Count != 0)
            {
                var MAresult = MessageBox.Show($"{InvalidChecked.Count} Songs assigned to checked locations were missing from your new song list!\n\n" +
                    $"Since these locations were already completed the song will not be replaced. The operation will continue, but records may be incomplete.\n\nWould you like to continue?\n\n" +
                    $"{InvalidChecked.Select(x => x.GetSongDisplayName(config)).ToFormattedJson()}", "Missing Songs!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (MAresult != DialogResult.Yes)
                    return;

                foreach (var i in InvalidChecked)
                    i.SongHash = string.Empty;
            }

            if (InvalidUnchecked.Count != 0)
            {
                var MUresult = MessageBox.Show($"{InvalidUnchecked.Count} Songs assigned to unchecked locations were missing from your new song list!\n\n" +
                    $"Would you like the client to attempt to replace these song with songs from the new list?\n\n" +
                    $"{InvalidUnchecked.Select(x => x.GetSongDisplayName(config)).ToFormattedJson()}", "Missing Required Songs!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (MUresult != DialogResult.Yes)
                    return;

                Dictionary<string, HashSet<string>> AssignedPerProfile = [];
                foreach(var i in ValidUnchecked.Concat(ValidChecked))
                {
                    AssignedPerProfile.SetIfEmpty(i.Requirements!.Name, []);
                    AssignedPerProfile[i.Requirements.Name].Add(i.SongHash!);
                }

                var AllProfiles = InvalidUnchecked.DistinctBy(i => i.Requirements!.Name).ToDictionary(i => i.Requirements!.Name, i => i.Requirements);
                foreach (var i in AllProfiles.Values)
                {
                    var ValidSongs = i!.GetAvailableSongs(data, AssignedPerProfile).Values.ToList();
                    var NeedingThisProfile = InvalidUnchecked.Where(x => x.Requirements!.Name == i.Name);
                    if (ValidSongs.Count < NeedingThisProfile.Count())
                    {
                        MessageBox.Show($"Not enough valid songs to assign to locations using profile {i.Name}\n\n" +
                            $"Needed {NeedingThisProfile.Count()} Found {ValidSongs.Count}. Please add more songs\n\n" +
                            $"{i.ToFormattedJson()}", "Rescan Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                foreach (var i in InvalidUnchecked)
                {
                    var ValidSongs = i.Requirements!.GetAvailableSongs(data, AssignedPerProfile).Values.ToList();
                    var NewCandidate = ValidSongs[connection.GetRNG().Next(ValidSongs.Count)];
                    Debug.WriteLine($"{NewCandidate.GetSongDisplayName()} Assigned to Song {i.SongNumber} replacing {i.GetSongDisplayName(config)}");
                    AssignedPerProfile.SetIfEmpty(i.Requirements.Name, []);
                    AssignedPerProfile[i.Requirements.Name].Add(NewCandidate.SongChecksum);
                    i.SongHash = NewCandidate.SongChecksum;
                }

            }
            config.SongData = new(data);
            config.SaveConfigFile(connection);

            string ReplacedMessage = InvalidUnchecked.Count != 0 ? $"Successfully replaced {InvalidUnchecked.Count} missing Songs\n\n" : "";
            MessageBox.Show($"Song List Rescanned.\n\n{ReplacedMessage}Previous Count: {CurrentCount} | New Count: {config.SongData.Count}", "Rescan Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;

        }
    }
}
