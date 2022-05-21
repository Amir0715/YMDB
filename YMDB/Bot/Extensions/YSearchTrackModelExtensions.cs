using System;
using System.Linq;

using Yandex.Music.Api.Models.Search.Track;

using YMDB.Bot.Extensions;

namespace YMDB.Bot.Utils
{
    public static class YSearchTrackModelExtensions
    {
        public static TimeSpan GetDuration(this YSearchTrackModel trackModel)
        {
            var durationSecond = trackModel.DurationMs / 1000;
            int seconds = (int)(durationSecond % 60);
            int minutes = (int)(durationSecond / 60);
            int hours = (int)(durationSecond / 60 / 60);
            var time = new TimeSpan(hours,minutes,seconds);
            return time;
        }
        
        public static string toString(this YSearchTrackModel trackModel)
        {
            return $"{trackModel.Title} - {trackModel.Artists.toString()}";
        }
        
        public static string GetLink(this YSearchTrackModel track)
        {
            return $"https://music.yandex.ru/album/{track.Albums.First().Id}/track/{track.Id}";
        }
    }
}