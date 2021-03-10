using System.Collections.Generic;
using DSharpPlus.Entities;
using Yandex.Music.Api.Models.Search.Track;
using YMDB.Bot.Utils;

namespace YMDB.Bot.Extensions
{
    public static class YSearchTrackModelList
    {
        public static (string str, DiscordEmbedBuilder embedBuilder) GetPages(this List<YSearchTrackModel> trackModels)
        {
            var listsongs = trackModels.toString();
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Search result")
                .AddField("Song count", trackModels.Count.ToString(), true)
                .WithColor(DiscordColor.Red)
                .WithDescription(listsongs);
            return (str: listsongs, embedBuilder: embed);
        }

        public static string toString(this List<YSearchTrackModel> trackModels)
        {
            var result = "";
            if (trackModels.Count == 0) result = "Nothing find!";
            var i = 0;
            foreach (var track in trackModels)
            {
                var trackduration =  track.GetDuration();
                result += $"`[{i++}]` | **" + track.toString() + $"** \t \t \t | `{trackduration.ToString()}`\n";
            }
            return result;
        }
    }
}