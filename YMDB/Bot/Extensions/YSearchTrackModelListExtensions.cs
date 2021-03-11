using System.Collections.Generic;
using System.Linq;
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
                .WithColor(DiscordColor.Red)
                .WithDescription(listsongs);
            return (str: listsongs, embedBuilder: embed);
        }
        
        public static (string str, DiscordEmbedBuilder embedBuilder) GetPage(this List<YSearchTrackModel> trackModels, int index, int startindex = 0)
        {
            var listsongs = trackModels.Skip(index * 10).Take(10).ToList().toString(startindex);
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Search result")
                .WithColor(DiscordColor.Red)
                .WithDescription(listsongs);
            return (str: listsongs, embedBuilder: embed);
        }

        public static string toString(this List<YSearchTrackModel> trackModels, int startindex = 0)
        {
            var result = "";
            if (trackModels.Count == 0) result = "Nothing find!";
            foreach (var track in trackModels)
            {
                var trackduration =  track.GetDuration();
                result += $"`[{startindex++}]` | **" + track.toString() + $"** \t \t \t | `{trackduration.ToString()}`\n";
            }
            return result;
        }
    }
}