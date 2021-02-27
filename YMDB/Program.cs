using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Yandex.Music.Api.Models.Common;
using Yandex.Music.Client;
using YMDB.Commands;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using YMDB.Bot.Commands;
using YMDB.Bot.Playlist;
using YMDB.Bot.Yandex;

namespace YMDB
{
    public class Program
    {
        public readonly EventId BotEventId = new EventId(42, "Bot-Music");
        public DiscordClient Discord { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public VoiceNextExtension Voice { get; set; }

        public YMDownloader YMD { get; set; }

        public static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            var json = "";
            using (var fs = File.OpenRead("BotConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                json = await sr.ReadToEndAsync();
            }
            
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            this.Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                AutoReconnect = true
            });

            this.Discord.Ready += this.Client_Ready;
            this.Discord.GuildAvailable += this.Client_GuildAvailable;
            this.Discord.ClientErrored += this.Client_ClientError;
            
            var services = new ServiceCollection()
                .AddSingleton<Dictionary<DiscordChannel,Playlist>>()
                .BuildServiceProvider();
            
            this.Commands = this.Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new []{ cfgjson.CommandPrefix },
                EnableMentionPrefix = true,
                Services = services
            });

            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;
            
            this.Commands.RegisterCommands<HelpModule>();
            this.Commands.RegisterCommands<MusicModule>();
            
            this.Voice =  this.Discord.UseVoiceNext();

            this.YMD = YMDownloader.GetInstance(cfgjson.Login, cfgjson.Password);
            
            await Discord.ConnectAsync();
            
            await Task.Delay(-1);
        }
        
        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");
            
            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }
        
        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }
        
        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError(
                BotEventId, 
                $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", 
                DateTime.Now
                );

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed);
            }
        }
        
    }
    
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
        
        [JsonProperty("login")]
        public string Login { get; private set; }
        
        [JsonProperty("password")]
        public string Password { get; private set; }
    }
}