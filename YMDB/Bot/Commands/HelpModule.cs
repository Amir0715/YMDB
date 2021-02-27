using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Yandex.Music.Client.Extensions;
using YMDB.Bot.Utils;
using YMDB.Bot.Yandex;

namespace YMDB.Commands
{
    public class HelpModule : BaseCommandModule
    {
        public Queue<string> Que { private get; set; }

        [Command]
        public async Task HelpCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("Пошел нахуй!");
        }
        
        [Command("save")]
        public async Task Save(CommandContext ctx, string url)
        {
            var ymd = YMDownloader.GetInstance();
            var track = UrlUtils.GetTrack(url);
            var filepath = ymd.DownloadTrack(url);
            
            var artists = track.Artists.toString();
            await ctx.RespondAsync($"Track `{artists} - {track.Title}` `[{track.Id}]` downloaded from `{track.GetLink()}`");
        }
        
    }
}