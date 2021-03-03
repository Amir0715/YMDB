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
using System.Threading;
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
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private CancellationToken _cancellationToken;

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
                        
                        await Add(ctx, url);
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
            if (vnc == null)
            {
                await ctx.RespondAsync("Bot isn't connected!");
                return;
            }

            Exception exc = null;
            try
            {
                if (vnc.IsPlaying)
                {
                    _cancelTokenSource.Cancel();
                }
            }
            catch (OperationCanceledException o)
            {
            }
            catch (Exception ex)
            {
                exc = ex;
            }
            finally
            {
                await vnc.SendSpeakingAsync(false);
            }
            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
            
            await PlayNextTrack(ctx,3);
        }
        
        [Command("join")]
        public async Task Join(CommandContext ctx, DiscordChannel channel = null)
        {
            // check whether VNext is enabled
            VoiceNextExtension vnext;

            if ((vnext = ctx.Client.GetVoiceNext()) == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);

            // get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            if (vnc?.TargetChannel == vstat.Channel)
            {
                await ctx.RespondAsync("Already in this voice channel.");
                return;
            }
            if (( vnc != null || vnc?.TargetChannel != channel ) && vnc?.TargetChannel != vstat.Channel)
            {
                Playlists.Remove(vnc.TargetChannel);
                vnc.Disconnect();
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
            Playlists.Remove(vnc.TargetChannel);
            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");
        }
        
        [Command("add")]
        public async Task Add(CommandContext ctx, string url)
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
                // not connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
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
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }
            
            var channel = vnc.TargetChannel;
            var interactivity = ctx.Client.GetInteractivity();
            
            var (str, embed) = Playlists[channel].GetPages();
            
            var pages = interactivity.GeneratePagesInEmbed(input: str, splittype: SplitType.Line, embedbase: embed);
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        }
        
        private async Task PlayFile(CommandContext ctx, string filepath)
        {
            ResetToken();
            
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
                // генерация OperationCanceledException при команде next 
                await vnc.SendSpeakingAsync(true);
                var ffout = FfmpegUtils.ConvertToPCM(filepath);
                
                var txStream = vnc.GetTransmitSink();

                await ffout.CopyToAsync(txStream, cancellationToken: _cancellationToken);
                await txStream.FlushAsync(_cancellationToken);
            }
            catch (OperationCanceledException o)
            {
            }
            catch (Exception ex)
            {
                exc = ex;
            }
            finally
            {
                _cancelTokenSource.Dispose();
                await vnc.SendSpeakingAsync(false);
            }
            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
            
        }

        [Command("stop")]
        private async Task Stop(CommandContext ctx)
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
            if (vnc == null)
            {
                await ctx.RespondAsync("Bot isn't connected!");
                return;
            }
            
            if (vnc.IsPlaying)
            {
                _cancelTokenSource.Cancel();
            }
        }
        private async Task PlayNextTrack(CommandContext ctx, float timeoutsec = 0)
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
            var startDateTime = DateTime.Now;
            while (vnc.IsPlaying && (DateTime.Now - startDateTime).TotalSeconds < timeoutsec)
            {
                Thread.Sleep(500);
            }
            if (vnc.IsPlaying)
            {
                await ctx.RespondAsync("Bot playing music");
            }
            else
            {
                // Переделать так что бы можно было скипать текущую песню
                if (Playlists[vnc.TargetChannel].GetCount() > 0)
                {
                    var track = Playlists[vnc.TargetChannel].GetNext();
                    var filepath = YMDownloader.GetInstance().DownloadTrack(track);

                    await ctx.RespondAsync(track.GetEmbedBuilder().WithUrl(track.GetLink()));

                    await PlayFile(ctx, filepath);
                    await vnc.WaitForPlaybackFinishAsync();

                    Playlists[vnc.TargetChannel].RemoveAt(0);
                }
                else
                {
                    await ctx.Message.RespondAsync($"Playlist is empty!");
                }
            }
        }
        private void ResetToken()
        {
            this._cancelTokenSource = new CancellationTokenSource();
            this._cancellationToken = this._cancelTokenSource.Token;
            _cancellationToken.Register(()=> this.ResetToken());
        }
        
    }
}