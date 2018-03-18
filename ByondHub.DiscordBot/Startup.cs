using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ByondHub.DiscordBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        private DiscordSocketClient _client;
        private ILogger _logger;
        private CommandService _commands;
        private IServiceProvider _services;

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");
            Configuration = builder.Build();
        }

        public static async Task RunAsync()
        {
            var startup = new Startup();
            await startup.StartAsync();
        }

        public async Task StartAsync()
        {
            var loggerFactory = new LoggerFactory().AddConsole();
            _logger = loggerFactory.CreateLogger("Bot");
            _commands = new CommandService();
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
#if DEBUG
                LogLevel = LogSeverity.Verbose
#else
                LogLevel = LogSeverity.Info
#endif
            });
            _client.Log += Log;
           
            InstallServices();
            await InstallCommandAsync();

            string token = Configuration["Bot:Token"];
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private void InstallServices()
        {
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_logger)
                .AddSingleton(Configuration)
                .AddSingleton<ServerService>()
                .BuildServiceProvider();
        }

        private async Task InstallCommandAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;
            if (msg.Author.Id == _client.CurrentUser.Id) return;

            int argPos = 0;
            string prefix = Configuration["Bot:Prefix"];
            bool hasStringPrefix;
            hasStringPrefix = !string.IsNullOrEmpty(prefix) &&
                              message.HasStringPrefix(prefix, ref argPos);
            bool hasMentionPrefix = message.HasMentionPrefix(_client.CurrentUser, ref argPos);

            if (!hasMentionPrefix && !hasStringPrefix)
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
            {
                if (Debugger.IsAttached)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
                _logger.LogError(result.ErrorReason);
            }

        }

        private Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(message.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(message.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(message.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogInformation(message.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(message.Message);
                    break;
                default:
                    _logger.LogInformation(message.Message);
                    break;

            }

            return Task.CompletedTask;
        }
    }
 }
