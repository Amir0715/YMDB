/*
 TODO: 
    Добавить методы для проигрывания\остановки\паузы музыки (по ссылке или названию).
    Доравботать метод проигрывания (автоподключение к каналу).
    Добавить поддержку проигрывания плейлистов\альбомов\артистов по ссылке.
      
    Добавить красивый вывод очереди и текущей песни (по возможности отделить цветами).
    Подумать над логированием бота в дебаг режиме.
    Поиск по названию\артисту\альбому\плейлисту с красивым выводом.
    Вывод обложки по возможности.
    
    МБ
    таймер отключения
*/



using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using YMDB.Bot.Utils;
using YMDB.Bot.Yandex;

namespace YMDB.Bot.Commands
{
    [ModuleLifespan(ModuleLifespan.Singleton)]
    public class MusicModule : BaseCommandModule
    {
        // [Command("play")]
        // public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string path)
        // {
        //     var vnext = ctx.Client.GetVoiceNext();
        //     if (vnext == null)
        //     {
        //         // not enabled
        //         await ctx.RespondAsync("VNext is not enabled or configured.");
        //         return;
        //     }
        //
        //     // check whether we aren't already connected
        //     var vnc = vnext.GetConnection(ctx.Guild);
        //     if (vnc == null)
        //     {
        //         // already connected
        //         await ctx.RespondAsync("Not connected in this guild.");
        //         return;
        //     }
        //     
        //     // check if file exists
        //     if (!File.Exists(path))
        //     {
        //         // file does not exist
        //         await ctx.RespondAsync($"File `{path}` does not exist.");
        //         return;
        //     }
        //     
        //     // wait for current playback to finish
        //     while (vnc.IsPlaying)
        //         await vnc.WaitForPlaybackFinishAsync();
        //     
        //     Exception exc = null;
        //     await ctx.Message.RespondAsync($"Playing `{path}`");
        //     
        //     try
        //     {
        //         await vnc.SendSpeakingAsync(true);
        //         
        //         var ffout = FfmpegUtils.ConvertToPCM(path);
        //
        //         var txStream = vnc.GetTransmitSink();
        //         await ffout.CopyToAsync(txStream);
        //         await txStream.FlushAsync();
        //         await vnc.WaitForPlaybackFinishAsync();
        //     }
        //     catch (Exception ex) { exc = ex; }
        //     finally
        //     {
        //         await vnc.SendSpeakingAsync(false);
        //         await ctx.Message.RespondAsync($"Finished playing `{path}`");
        //     }
        //
        //     if (exc != null)
        //         await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        // }

        public Dictionary<DiscordChannel, Playlist.Playlist> Playlists { private get; set; }

        [Command("play")]
        public async Task Play(CommandContext ctx, [Description("Ссылка трека/плейлиста/альбома")] string url)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // already connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            Exception exc = null;
            try
            {
                var type = UrlUtils.GetTypeOfUrl(url);
                switch (type)
                {
                    case UrlUtils.TypeOfUrl.NONE: 
                        await ctx.RespondAsync("Incorrect url. Please try again or get help via `~help`.");
                        return;
                    case UrlUtils.TypeOfUrl.TRACK:
                        var track = UrlUtils.GetTrack(url);
                        await ctx.RespondAsync($"Track is `{track.Title}` - `{track.Artists.toString()}`.");
                        // TODO: Логика проигрыввания трека или добавления его в очередь.
                        // Наверное лучше реализовать функцию проигрывания файла отдельно с сигнатурой
                        // private async (void?) PlayFile(DiscordContext ctx, string path)
                        var filepath = YMDownloader.GetInstance().DownloadTrack(url);
                        await PlayFile(ctx, vnc, filepath);
                        break;
                    case UrlUtils.TypeOfUrl.ALBUM:
                        var album = UrlUtils.GetAlbum(url);
                        await ctx.RespondAsync($"Album is `{album.Title}` - `{album.Volumes.Count}`");
                        break;
                    case UrlUtils.TypeOfUrl.ARTIST: break;
                    case UrlUtils.TypeOfUrl.PLAYLIST: break;
                }
            }
            catch (Exception ex) { exc = ex; }
            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }
        
        [Command("join")]
        public async Task Join(CommandContext ctx, DiscordChannel channel = null)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                // already connected
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }

            // get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && channel == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            // channel not specified, use user's
            if (channel == null)
                channel = vstat.Channel;

            // connect
            await vnext.ConnectAsync(channel);
            Playlists.Add(channel, new Playlist.Playlist());
            await ctx.RespondAsync($"Connected to `{channel.Name}`");
        }
        
        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we are connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // not connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // disconnect
            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");
        }
        
        [Command("add")]
        public async Task Add(CommandContext ctx, string url)
        {
            var channel = ctx.Member.VoiceState.Channel;
            var track = UrlUtils.GetTrack(url);
            Playlists[channel].AddToEnd(track);
            await ctx.RespondAsync(
                $"`{track.Title} - {track.Artists.toString()}` added to playlist channel `{channel.Name}`");
        }
        
        [Command("list")]
        public async Task List(CommandContext ctx)
        {
            var channel = ctx.Member.VoiceState.Channel;

            await ctx.RespondAsync(Playlists[channel]?.ToString());
        }
        
        private async Task PlayFile(CommandContext ctx, VoiceNextConnection vnc, string filepath)
        {
            if (vnc.IsPlaying)
            {
                
            }

            Exception exc = null;
            try
            {
                await vnc.SendSpeakingAsync(true);
                
                var ffout = FfmpegUtils.ConvertToPCM(filepath);
            
                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
                await ctx.Message.RespondAsync($"Finished playing `{filepath}`");
            }
            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
            
        }
        
    }
}