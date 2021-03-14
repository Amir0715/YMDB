using System;
using System.Collections;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace YMDB.Bot.Utils
{
    public static class PlaylistExtensions
    {
        public static DiscordEmbedBuilder GetEmbedBuilder(this Playlist.Playlist playlist)
        {
            var listsongs = playlist.ToString();
            var duration = playlist.GetDuration();

            return new DiscordEmbedBuilder()
                .WithTitle("Current playlist")
                .AddField("Duration", duration.ToString(), true)
                .AddField("Song count", playlist.GetCount().ToString(), true)
                .WithColor(DiscordColor.Cyan)
                .WithDescription(listsongs);
        }
        
        public static (string str, DiscordEmbedBuilder embedBuilder) GetPages(this Playlist.Playlist playlist)
        {
            var listsongs = playlist.ToString();
            var duration = playlist.GetDuration();

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Current playlist")
                .AddField("Duration", duration.ToString(), true)
                .AddField("Song count", playlist.GetCount().ToString(), true)
                .WithColor(DiscordColor.Cyan);
            return (str: listsongs ,embedBuilder: embed);
        }
    }
}