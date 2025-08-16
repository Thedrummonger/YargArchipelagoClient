using System;
using System.Collections.Generic;
using YARG.Core.Song;
using YARG.Core.Utility;
using YARG.Core;
using YargArchipelagoCommon;

namespace YargArchipelagoPlugin
{
    public static class YargAPUtils
    {
        public static bool IsSupportedInstrument(Instrument source, out CommonData.SupportedInstrument? target)
        {
            int value = (int)source;
            if (Enum.IsDefined(typeof(CommonData.SupportedInstrument), value))
            {
                target = (CommonData.SupportedInstrument)value;
                return true;
            }
            target = default;
            return false;
        }
        public static CommonData.SupportedDifficulty GetSupportedDifficulty(Difficulty source)
        {
            if (source > Difficulty.Expert)
                return CommonData.SupportedDifficulty.Expert;
            if (source < Difficulty.Easy)
                return CommonData.SupportedDifficulty.Easy;
            return (CommonData.SupportedDifficulty)(int)source;
        }
        public static CommonData.SongData ToSongData(this SongEntry song)
        {
            return new CommonData.SongData()
            {
                Album = RichTextUtils.StripRichTextTags(song.Album),
                Artist = RichTextUtils.StripRichTextTags(song.Artist),
                Charter = RichTextUtils.StripRichTextTags(song.Charter),
                Name = RichTextUtils.StripRichTextTags(song.Name),
                Path = song.ActualLocation,
                SongChecksum = Convert.ToBase64String(song.Hash.HashBytes),
                Difficulties = new Dictionary<CommonData.SupportedInstrument, int>()
            };
        }
    }
}
