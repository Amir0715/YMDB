using System;
using System.Linq;
using DSharpPlus.Entities;
using Yandex.Music.Api.Models.Track;

namespace YMDB.Bot.Utils
{
    public static class YTrackExtensions
    {
        public static DiscordEmbedBuilder GetEmbedBuilder(this YTrack track)
        {
            return new DiscordEmbedBuilder()
                .WithTitle($"{track.Title}")
                .WithImageUrl($"http://{track.CoverUri.Replace("%%", "1000x1000")}")
                .WithColor(DiscordColor.Aquamarine)
                .AddField("Artists", $"{track.Artists.toString()}", true)
                .AddField("Album:", $"{track.Albums.toString()}", true)
                .AddField("Durations", $"{track.GetDuration().ToString()}", true);
        }

        public static TimeSpan GetDuration(this YTrack track)
        {
            var durationSecond = track.DurationMs / 1000;
            int seconds = (int)(durationSecond % 60);
            int minutes = (int)(durationSecond / 60);
            int hours = (int)(durationSecond / 60 / 60);
            var time = new TimeSpan(hours,minutes,seconds);
            return time;
        }

        public static string toString(this YTrack track)
        {
            return $"{track.Title} - {track.Artists.toString()}";
        }

        public static string GetLink(this YTrack track)
        {
            return $"https://music.yandex.ru/album/{track.Albums.First().Id}/track/{track.Id}";
        }
    }
}