using System;
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
            int seconds = (int)(track.DurationMs/1000 % 60);
            int minutes = (int)(track.DurationMs/1000 / 60);
            int hours = (int)(track.DurationMs/1000 / 60 / 60);
            var time = new TimeSpan(hours,minutes,seconds);
            return time;
        }

        public static string toString(this YTrack track)
        {
            return $"{track.Title} - {track.Artists.toString()}";
        }
    }
}