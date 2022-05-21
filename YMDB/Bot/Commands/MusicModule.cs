using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;

using Yandex.Music.Api.Models.Common;

using YMDB.Bot.CustomPaginationRequests;
using YMDB.Bot.Extensions;
using YMDB.Bot.Utils;
using YMDB.Bot.Yandex;

namespace YMDB.Bot.Commands
{
    [ModuleLifespan(ModuleLifespan.Singleton)]
    public class MusicModule : BaseCommandModule
    {
        public Dictionary<DiscordChannel, Playlist.Playlist> Playlists { private get; set; }

        private CancellationTokenSource _cancelTokenSource = new();
        private CancellationToken _cancellationToken;

        [Command("play"), Aliases("p"), Description("Play track/playlist/artist's songs/album from yandex.music.")]
        public async Task Play(CommandContext ctx, [Description("Url of track/playlist/artist's songs/album or title of track."), RemainingText] string url)
        {
            // Подумать над "фикстурами"
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
                await Join(ctx);
                vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null)
                    return;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                await ctx.RespondAsync("Аргумент команды пустой");
                return;
            }

            Exception exc = null;
            try
            {
                var type = UrlUtils.GetTypeOfUrl(url);

                switch (type)
                {
                    case UrlUtils.TypeOfUrl.NONE:
                        await Search(ctx, url);

                        break;

                    case UrlUtils.TypeOfUrl.TRACK:

                        var track = UrlUtils.GetTrack(url);
                        Playlists[vnc.TargetChannel].AddToEnd(track);
                        break;
                    case UrlUtils.TypeOfUrl.ALBUM:

                        var album = UrlUtils.GetAlbum(url);
                        Playlists[vnc.TargetChannel].AddToEnd(album);

                        break;
                    case UrlUtils.TypeOfUrl.ARTIST:

                        var artist = UrlUtils.GetArtistBriefInfo(url);
                        Playlists[vnc.TargetChannel].AddToEnd(artist);

                        break;
                    case UrlUtils.TypeOfUrl.PLAYLIST:

                        var playlist = UrlUtils.GetPlaylist(url);
                        Playlists[vnc.TargetChannel].AddToEnd(playlist);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await Play(ctx);
            }
            catch (Exception ex) { exc = ex; }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }

        [Command("search"), Aliases("se"), Description("Search track by title from yandex.music.")]
        public async Task Search(CommandContext ctx, [Description("Track title."), RemainingText] string title)
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
                await Join(ctx);
                vnc = vnext.GetConnection(ctx.Guild);
            }
            
            if (string.IsNullOrWhiteSpace(title))
            {
                await ctx.RespondAsync("Аргумент команды пустой");
                return;
            }

            var tracks = YMDownloader.GetInstance().Ymc.Search(title, YSearchType.Track).Tracks.Results;

            var interactivity = ctx.Client.GetInteractivity();

            var (str, embed) = tracks.GetPage(0);

            var pages = interactivity.GeneratePagesInEmbed(input: str,
                splittype: SplitType.Line, embedbase: embed);

            var message = await new DiscordMessageBuilder().WithContent(pages.First().Content)
                .WithEmbed(pages.First().Embed)
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);

            var customPage = new CustomPaginationRequest(message, ctx.Member,
                PaginationBehaviour.Ignore, PaginationDeletion.DeleteEmojis, new PaginationEmojis(),
                TimeSpan.FromMinutes(1), pages.ToArray());

            customPage.SetEnumerator(GetNextPage(interactivity, title).GetEnumerator());
            var task = interactivity.WaitForCustomPaginationAsync(customPage);

            await ctx.RespondAsync("Enter the index of track: ");

            var res = await interactivity.WaitForMessageAsync(m => int.TryParse(m.Content.Trim(), out var x), TimeSpan.FromMinutes(1));

            if (!res.TimedOut)
            {
                await ctx.RespondAsync("I got your answer : " + res.Result.Content);

                int.TryParse(res.Result.Content, out var x);

                var pageIndex = x / 20;
                var trackIndexInPage = x % 20;

                var tracksResults = YMDownloader.GetInstance().Ymc.Search(title, YSearchType.Track, pageIndex).Tracks.Results;

                await Add(ctx, tracksResults[trackIndexInPage].GetLink());
                // if (!vnc.IsPlaying)
                // {
                //     Play(ctx);
                // }
            }
        }

        [Command("next"), Aliases("n"), Description("Next track.")]
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
                if (vnc.IsPlaying) { _cancelTokenSource.Cancel(); }
            }
            catch (OperationCanceledException)
            {
                //await vnc.SendSpeakingAsync(false);
            }
            catch (Exception ex) { exc = ex; }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");

            //await PlayNextTrack(ctx,3);
        }

        [Command("nowplaying"), Aliases("np"), Description("Shows what track is currently playing.")]
        public async Task NowPlaying(CommandContext ctx)
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

            if (!vnc.IsPlaying)
            {
                await ctx.RespondAsync("Bot doesn't playing!");
                return;
            }

            var track = Playlists[vnc.TargetChannel].GetNext();
            await ctx.RespondAsync(track.GetEmbedBuilder().WithUrl(track.GetLink()));

        }

        [Command("skip"), Aliases("sk"), Description("Skip count track and play count+1 track.")]
        public async Task Skip(CommandContext ctx, [Description("Count track, default is 1.")] int count = 1)
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

            if (!vnc.IsPlaying)
            {
                await ctx.RespondAsync("Bot doesn't playing!");
                return;
            }

            if (count <= 0)
            {
                await ctx.RespondAsync("Count can't be <= 0");
                return;
            }

            Playlists[vnc.TargetChannel].Skip(count - 1);
            await Next(ctx);
            // await PlayNextTrack(ctx,3);
        }

        [Command("clear"), Aliases("cl"), Description("Clear current playlist.")]
        public async Task Clear(CommandContext ctx)
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

            if (Playlists[vnc.TargetChannel].GetCount() == 0)
            {
                await ctx.RespondAsync("Playlist is clear!");
                return;
            }

            Playlists[vnc.TargetChannel].Clear();

        }

        [Command("join"), Aliases("j"), Description("Join to voice channel.")]
        public async Task Join(CommandContext ctx, [Description("Voice channel.")] DiscordChannel channel = null)
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

            if ((vnc != null || vnc?.TargetChannel != channel) && vnc?.TargetChannel != vstat.Channel)
            {
                Playlists.Remove(vnc.TargetChannel);
                vnc.Disconnect();
            }

            // channel not specified, use user's
            if (channel == null)
                channel = vstat.Channel;

            // connect
            await channel.ConnectAsync();
            Playlists.Add(channel, new Playlist.Playlist());
            await ctx.RespondAsync($"Connected to `{channel.Name}`");
        }

        [Command("leave"), Aliases("le"), Description("Leave from channel.")]
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

        [Command("add"), Aliases("a"), Description("Add track to end of playlist.")]
        public async Task Add(CommandContext ctx, [Description("Track url.")] string url)
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

        [Command("list"), Aliases("l"), Description("Show current playlist.")]
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

        [Command("stop"), Aliases("s"), Description("Stop playing.")]
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
                Playlists[vnc.TargetChannel].Clear();
            }
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
                await vnc.SendSpeakingAsync();
                await using var ffout = FfmpegUtils.ConvertToPcm(filepath);
                var txStream = vnc.GetTransmitSink();

                await ffout.CopyToAsync(txStream, cancellationToken: _cancellationToken);

                await txStream.FlushAsync(_cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { exc = ex; }
            finally
            {
                _cancelTokenSource.Dispose();
                await vnc.SendSpeakingAsync(false);
            }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }

        private async Task PlayNextTrack(CommandContext ctx, float timeoutsec = 0)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);

            var startDateTime = DateTime.Now;
            while (vnc.IsPlaying && (DateTime.Now - startDateTime).TotalSeconds < timeoutsec) { Thread.Sleep(500); }

            if (vnc.IsPlaying) { await ctx.RespondAsync("Bot playing music"); } else
            {
                if (Playlists[vnc.TargetChannel].GetCount() > 0)
                {
                    var track = Playlists[vnc.TargetChannel].GetNext();
                    var filepath = YMDownloader.GetInstance().DownloadTrack(track);

                    await ctx.RespondAsync(track.GetEmbedBuilder().WithUrl(track.GetLink()));

                    await PlayFile(ctx, filepath);
                    await vnc.WaitForPlaybackFinishAsync();

                    Playlists[vnc.TargetChannel].RemoveAt(0);
                } else { await ctx.Message.RespondAsync("Playlist is empty!"); }
            }
        }

        private async Task Play(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("vnc is null");
                return;
            }

            if (Playlists[vnc.TargetChannel] == null || Playlists[vnc.TargetChannel].GetCount() == 0)
            {
                await ctx.RespondAsync("Для данного канала не существует плейлиста!");
                return;
            }

            if (vnc.IsPlaying)
            {
                await ctx.RespondAsync("Bot playing music");
                return;
            }

            while (Playlists[vnc.TargetChannel].GetCount() > 0) { await PlayNextTrack(ctx); }

            await ctx.Message.RespondAsync("Playlist is empty!");
        }

        private void ResetToken()
        {
            _cancelTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancelTokenSource.Token;
            _cancellationToken.Register(() => ResetToken());
        }

        private IEnumerable<Page> GetNextPage(InteractivityExtension interactivity, string title)
        {
            var startindex = 0;
            var total = YMDownloader.GetInstance().Ymc.Search(title, YSearchType.Track).Tracks.Total;

            if (total != null)
            {
                var tmp = YMDownloader.GetInstance().Ymc.Search(title, YSearchType.Track).Tracks.Results;

                startindex += 10;
                var (str, embed) = tmp.GetPage(1, startindex);
                var page = interactivity.GeneratePagesInEmbed(input: str,
                    splittype: SplitType.Line, embedbase: embed);

                yield return page.First();

                for (var i = 1; i < (int)total / 20; i++)
                {
                    tmp = YMDownloader.GetInstance().Ymc.Search(title, YSearchType.Track, i).Tracks.Results;

                    startindex += 10;
                    (str, embed) = tmp.GetPage(0, startindex);
                    page = interactivity.GeneratePagesInEmbed(input: str,
                        splittype: SplitType.Line, embedbase: embed);

                    yield return page.First();

                    startindex += 10;
                    (str, embed) = tmp.GetPage(1, startindex);
                    page = interactivity.GeneratePagesInEmbed(input: str,
                        splittype: SplitType.Line, embedbase: embed);

                    yield return page.First();
                }
            }
        }
    }
}
