using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Preconditions;
using ByondHub.DiscordBot.Core.Server.Services;
using Discord;
using Discord.Commands;
using Discord.Net;
using Microsoft.Extensions.Configuration;

namespace ByondHub.DiscordBot.Core.Server.Modules
{
    [Group("server")]
    [RequireWhitelistedRole]
    public class ServerModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServerRequester _server;
        private readonly StatusService _statusUpdater;
        private readonly IConfiguration _config;

        public ServerModule(IServerRequester requester, StatusService statusUpdater, IConfiguration config)
        {
            _server = requester;
            _statusUpdater = statusUpdater;
            _config = config;
        }

        [Command("start")]
        [Summary("Starts server. Provide id and port.")]
        public async Task StartServerAsync([Summary("Server id")] string id, [Summary("Port to start")] int port)
        {
            var res = await _server.SendStartRequestAsync(id, port);
            if (res.Error)
            {
                await ReplyAsync($"Server '{res.Id}' error: {res.ErrorMessage}");
                return;
            }
            await ReplyAsync($"Server '{res.Id}': {res.Message} Port: {res.Port}");
        }

        [Command("stop")]
        [Summary("Stops server. Provide id")]
        public async Task StopServerAsync([Summary("Server id")] string id)
        {
            var res = await _server.SendStopRequestAsync(id);

            if (res.Error)
            {
                await ReplyAsync($"Server '{res.Id}' error: {res.ErrorMessage}");
                return;
            }

            await ReplyAsync($"Server '{res.Id}': {res.Message}");
        }

        [Command("update", RunMode = RunMode.Async)]
        [Summary("Updates server. Provide id.")]
        public async Task UpdateServerAsync([Summary("Server id")] string id,
            [Summary("Server branch name. Optional")] string branchName = "master",
            [Summary("Server commit hash. Optional")] string commitHash = "")
        {
            await ReplyAsync($"Sent update request for \"{id}\"...");
            var updateResult = await _server.SendUpdateRequestAsync(id, branchName, commitHash);
            if (updateResult.Error)
            {
                await ReplyAsync($"Update request for \"{id}\" is finished. Got error: {updateResult.ErrorMessage}");
                return;
            }

            if (updateResult.UpToDate)
            {
                await ReplyAsync(
                    $"Update request for \"{id}\" is finished. " +
                    $"Server is up-to-date on branch \"{updateResult.Branch}\" and " +
                    $"on commit \"{updateResult.CommitHash}\" ({updateResult.CommitMessage}).");
                return;
            }

            await ReplyAsync($"Server \"{id}\" was compiled" +
                             $" on branch \"{updateResult.Branch}\" and on commit \"{updateResult.CommitHash}\" ({updateResult.CommitMessage}).\n" +
                             $"Build log:\n{updateResult.Output}");
        }

        [Command("worldlog")]
        [Summary("Gets server world.log.")]
        public async Task GetServerWorldLogAsync([Summary("Server id")] string id)
        {
            try
            {
                var result = await _server.SendWorldLogRequestAsync(id);
                await ReplyAsync("Check your DM.");
                var dm = await Context.Message.Author.GetOrCreateDMChannelAsync();
                if (result.Error)
                {
                    await dm.SendMessageAsync($"Server '{result.Id}' got error: {result.ErrorMessage}");
                    return;
                }

                await dm.SendFileAsync(result.LogFileStream, $"{id}.log");
            }
            catch (HttpException e)
            {
                await ReplyAsync($"Got exception: ${e.Reason}");
            }

        }

        [Command("status", RunMode = RunMode.Async)]
        [Summary("Gets server status.")]
        public async Task GetServerStatus([Summary("Server id.")] string id)
        {
            await ReplyAsync("Updating server status...");
            try
            {
                var status = await _server.SendStatusRequestAsync(id);
                id = id.ToUpper();
                if (status.Error)
                {
                    await ReplyAsync($"{id} error: {status.ErrorMessage}");
                }
                else if (status.IsUpdating)
                {
                    await ReplyAsync($"{id} is updating. Check last update log with command \"``server buildlog {id.ToLower()}``\"");
                }
                else if (status.IsRunning)
                {
                    await ReplyAsync($"{id} is running.\n" +
                                     $"Players: {status.Players}\n" +
                                     $"Admins: {status.Admins}\n" +
                                     $"Join now: {status.Address}:{status.Port}");
                }
                else
                {
                    await ReplyAsync($"{id} is offline");
                }

            }
            catch (HttpException e)
            {
                await ReplyAsync($"Got exception: ${e.Reason}");
            }
        }

        [Command("buildlog", RunMode = RunMode.Async)]
        [Summary("Get server build log.")]
        public async Task GetBuildLog([Summary("Server id")] string id)
        {
            var status = await _server.SendStatusRequestAsync(id);
            if (status.Error)
            {
                await ReplyAsync($"Got error: {status.ErrorMessage}");
            }
            else if (status.IsUpdating)
            {
                await ReplyAsync($"Server '{id}' is updating. Please check build log with command \"``server buildlog {id.ToLower()}``\" later");
            }
            else if (string.IsNullOrEmpty(status.LastBuildLog))
            {
                await ReplyAsync($"Build log for '{id}' is empty.");
            }
            else
            {
                await ReplyAsync($"Build log for '{id}':\n{status.LastBuildLog}");
            }
        }

      
        [Command("status-start")]
        public async Task SetStatusUpdate([Summary("Server id")] string id)
        {
            var statusChannelId = _config.GetSection("Bot:InfoChannelId").Get<ulong>();
            var channel = Context.Guild.GetTextChannel(statusChannelId);

            if (channel == null)
            {
                await ReplyAsync("Status channel not found. Check your config.");
            }

            var result = await _statusUpdater.StartUpdatingAsync(id, channel);
            if (result.Error)
            {
                await ReplyAsync($"Error: {result.ErrorMessage}");
            }
            else
            {
                await ReplyAsync(result.Message);
            }
        }

        [Command("status-stop")]
        public async Task StopStatusUpdate([Summary("Server id.")] string id)
        {
            var result = await _statusUpdater.StopUpdatingAsync(id);
            if (result.Error)
            {
                await ReplyAsync($"Error: {result.ErrorMessage}");
            }
            else
            {
                await ReplyAsync(result.Message);
            }
        }
    }
}

