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
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
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
                await ctx.RespondAsync("VNext не подключен или настроен.");
                return;
            }
            
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // yet don't connected
                // await ctx.RespondAsync("Not connected in this guild.");
                await this.Join(ctx);
                vnc = vnext.GetConnection(ctx.Guild);
            }
            Exception exc = null;
            try
            {
                string filepath = null;
                var type = UrlUtils.GetTypeOfUrl(url);
                
                if (vnc == null)
                {
                    await ctx.RespondAsync("vnc is null");
                    return;
                }
                
                switch (type)
                {
                    case UrlUtils.TypeOfUrl.NONE: 
                        // Если ссылка нам не знакома
                        await ctx.RespondAsync("Не могу определить ссылку, попробуйте еще раз или введите `~help`.");
                        return;
                    
                    case UrlUtils.TypeOfUrl.TRACK:
                        // Если ссылка на трек
                        var track = UrlUtils.GetTrack(url);
                        
                        //Playlists[ctx.Member.VoiceState.Channel].AddToBegin(track);
                        await Add(ctx, url);
                        
                        /*
                         * 1. добавить песню в начало плейлиста
                         * 2. прорека играет ли музыка
                         * 3. если играет - ничего не проигрываем
                         * 4. иначе проигрываем трек из плейлиста
                         */
                        // if (vnc.IsPlaying)
                        // {
                        //     await ctx.RespondAsync($"Трек `{track.toString()}` добавлен в очередь.");
                        // }
                        // else
                        // {
                        //     filepath = YMDownloader.GetInstance().DownloadTrack(url);
                        //     await ctx.RespondAsync(track.GetEmbedBuilder().WithUrl(url));
                        //     
                        //     await PlayFile(ctx, filepath);
                        // }
                        await PlayNextTrack(ctx);
                        
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

        [Command("next")]
        public async Task Next(CommandContext ctx)
        {
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
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Bot not in channel!");
            }

            var channel = vnc.TargetChannel;
            var track = UrlUtils.GetTrack(url);
            
            Playlists[channel].AddToEnd(track);
            
            await ctx.RespondAsync(
                $"`{track.Title} - {track.Artists.toString()}` added to `{channel.Name}` playlist's !");
        }
        
        [Command("list")]
        public async Task List(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Bot not in channel!");
            }

            var channel = vnc.TargetChannel;
            var interactivity = ctx.Client.GetInteractivity();
            
            var (str, embed) = Playlists[channel].GetPages();
            
            var pages = interactivity.GeneratePagesInEmbed(input: str, splittype: SplitType.Line, embedbase: embed);
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        }
        
        private async Task PlayFile(CommandContext ctx, string filepath)
        {
            var vnc = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild);
            if (filepath == null)
            {
                await ctx.RespondAsync("Can't find file");
                return;
            }

            if (vnc == null)
            {
                await ctx.RespondAsync("vnc is null");
                return;
            }
            // TODO : Наверное стоит убрать этот трай или трай в методе плэй
            Exception exc = null;

            try
            {
                await vnc.SendSpeakingAsync(true);
                var ffout = FfmpegUtils.ConvertToPCM(filepath);
            
                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
            }
            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
            
        }

        private async Task PlayNextTrack(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("vnc is null");
                return;
            }
            if (Playlists[vnc.TargetChannel] == null)
            {
                await ctx.RespondAsync("Для данного канала не существует плейлиста!");
                return;
            }
            if (vnc.IsPlaying)
            {
                await ctx.RespondAsync("Bot playing music");
                return;
            }
            else
            {
                while (Playlists[vnc.TargetChannel].GetCount() > 0)
                {
                    var track = Playlists[vnc.TargetChannel].GetNext();
                    var filepath = YMDownloader.GetInstance().DownloadTrack(track);
                    await ctx.RespondAsync(track.GetEmbedBuilder().WithUrl(track.GetLink()));
                    await PlayFile(ctx, filepath);
                    await vnc.WaitForPlaybackFinishAsync();
                    Playlists[vnc.TargetChannel].RemoveAt(0);
                    await ctx.Message.RespondAsync($"Finished playing `{track.toString()}`");
                }
                await ctx.Message.RespondAsync($"Playlist is empty!");
                
            }
        }
        
    }
}