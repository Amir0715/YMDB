using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using YMDB.Bot.Commands;
using YMDB.Bot.Utils;
using YMDB.Bot.Yandex;

namespace YMDB.Bot
{
    public class Bot
    {

        #region fields
        
        public readonly EventId BotEventId = new(42, "Bot-Music");
        private DiscordClient Discord;
        private CommandsNextExtension Commands;
        private VoiceNextExtension Voice;
        private ServiceProvider Services;
        private YMDownloader YMD;
        
        #endregion

        #region constuctors
        
        public Bot(ConfigJson config)
        {
            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = config.DiscordToken,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                AutoReconnect = true
            });
            
            Services = new ServiceCollection()
                .AddSingleton<Dictionary<DiscordChannel, Playlist.Playlist>>()
                .BuildServiceProvider();
            
            Commands = Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new []{ config.CommandPrefix },
                EnableMentionPrefix = true,
                Services = Services
            });
            
            Discord.UseInteractivity(new InteractivityConfiguration {
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            
            // регистрация ивентов 
            Discord.Ready += Client_Ready;
            Discord.GuildAvailable += Client_GuildAvailable;
            Discord.ClientErrored += Client_ClientError;
            
            // регистрация командных исполнителей 
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;
            
            // регистрация коммандых модулей 
            Commands.RegisterCommands<MusicModule>();
            
            Voice =  Discord.UseVoiceNext();
            
            YMD 
                = string.IsNullOrWhiteSpace(config.YandexToken) 
                    ? YMDownloader.GetInstance(config.Login, config.Password, config.DownloadPath)
                    : YMDownloader.GetInstance(config.YandexToken, config.DownloadPath);
            
        }
        
        #endregion
        
        #region start/stop
        
        public async Task StartAsync()
        {
            await Discord.ConnectAsync();
            await Task.Delay(-1);
        }
        
        public async Task StopAsync()
            => await Discord.DisconnectAsync();

        #endregion

        #region Events
        
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
        #endregion

        #region Executers
        
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
        
        
        #endregion

    }
    
}
