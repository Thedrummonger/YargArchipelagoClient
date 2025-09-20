using System;
using System.Collections.Generic;
using System.Linq;
using YARG.Core.Game;
using YARG.Gameplay;
using YARG.Scores;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData.Networking;

namespace YargArchipelagoPlugin
{
    public class ArchipelagoEventManager
    {
        public ArchipelagoService APHandler { get; private set; }
        public ArchipelagoEventManager(ArchipelagoService Handler)
        {
            APHandler = Handler;
        }
        public void SendSongCompletionResults(List<PlayerScoreRecord> playerScoreRecords, GameRecord record)
        {
            APHandler.Log($"Recording Score for AP");
            var songPassInfo = new CommonData.SongPassInfo(Convert.ToBase64String(record.SongChecksum));
            songPassInfo.participants = playerScoreRecords
                .Where(x => YargAPUtils.IsSupportedInstrument(x.Instrument, out _))
                .Select(x => new CommonData.SongParticipantInfo()
                {
                    Difficulty = YargAPUtils.GetSupportedDifficulty(x.Difficulty),
                    instrument = YargAPUtils.IsSupportedInstrument(x.Instrument, out var SupportedInstrument) ? SupportedInstrument : null,
                    FC = x.IsFc,
                    Percentage = x.Percent ?? 0,
                    Score = x.Score,
                    Stars = StarAmountHelper.GetStarCount(x.Stars),
                    WasGoldStar = x.Stars == StarAmount.StarGold,
                }).ToArray();
            songPassInfo.SongPassed = true; //Always true for now, will be handled when yarg implements fail mode

            _ = APHandler.packetClient?.SendPacketAsync(new YargAPPacket { passInfo = songPassInfo });
        }

        public void SongStarted(GameManager gameManager)
        {
            APHandler.SetCurrentSong(gameManager);
            _ = APHandler.packetClient?.SendPacketAsync(new YargAPPacket
            {
                CurrentlyPlaying = CommonData.CurrentlyPlayingData.CurrentlyPlayingSong(gameManager.Song.ToSongData())
            });
        }
        public void SongEnded()
        {
            APHandler.ClearCurrentSong();
            APHandler.HasAvailableSongUpdate = true;
            _ = APHandler.packetClient?.SendPacketAsync(new YargAPPacket
            {
                CurrentlyPlaying = CommonData.CurrentlyPlayingData.CurrentlyPlayingNone()
            });
        }
        public void SongFailed(GameManager gameManager)
        {
            _ = APHandler.packetClient?.SendPacketAsync(new YargAPPacket
            {
                songFailData = new CommonData.SongFailData(gameManager.Song.ToSongData())
            });
        }
    }
}
