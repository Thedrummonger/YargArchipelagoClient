using System.Diagnostics;
using System.Text.RegularExpressions;

namespace YargArchipelagoClient.Data
{
    public class SongLoader
    {
        /// <summary>
        /// Loads all song.ini files starting from the specified root directory.
        /// </summary>
        /// <param name="rootFolder">The root folder to begin searching.</param>
        /// <returns>
        /// A master dictionary where each key is the song name (from the ini file's "name" key)
        /// and the value is a SongData object containing all key/value pairs from that file.
        /// </returns>
        public static Dictionary<string, SongData> LoadSongs(string rootFolder)
        {
            Dictionary<string, SongData> songs = [];
            string[] iniFiles = Directory.GetFiles(rootFolder, "song.ini", SearchOption.AllDirectories);

            foreach (var iniFile in iniFiles)
            {
                string[] lines = File.ReadAllLines(iniFile);
                Dictionary<string, string> songData = [];

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]")))
                        continue;
                    int separatorIndex = trimmedLine.IndexOf('=');
                    if (separatorIndex == 0) 
                        continue;
                    string key = trimmedLine[..separatorIndex].Trim();
                    string value = trimmedLine[(separatorIndex + 1)..].Trim();
                    // Remove HTML-like tags from the value (e.g., <color=#B900FF> ... </color>).
                    value = Regex.Replace(value, "<.*?>", string.Empty);
                    songData[key] = value;
                }
                if (!songData.TryGetValue("name", out string? songName))
                {
                    Debug.WriteLine($"Warning: File {iniFile} does not contain a 'name' field.");
                    continue;
                }
                if (songs.ContainsKey(songName))
                {
                    Debug.WriteLine($"Warning: Duplicate song name encountered: {songName}");
                    continue;
                }
                songs.Add(songName, new SongData(songData));
            }

            return songs;
        }
    }
}
