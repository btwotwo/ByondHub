using System.Text;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Preconditions;
using ByondHub.DiscordBot.Core.Server.Services;
using Discord;
using Discord.Commands;
using Discord.Net;

namespace ByondHub.DiscordBot.Core.Server.Modules
{
    [Group("server")]
    [RequireWhitelistedRole]
    public class ServerModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServerRequester _server;

        public ServerModule(IServerRequester requester)
        {
            _server = requester;
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

            await ReplyAsync($"Update request for \"{id}\" is finished: {updateResult.Output}\n" +
                             $"Server is on branch \"{updateResult.Branch}\" and on commit \"{updateResult.CommitHash}\" ({updateResult.CommitMessage}).");
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
            var msg = await ReplyAsync("Updating server status...");
            try
            {
                var status = await _server.SendStatusRequestAsync(id);
                id = id.ToUpper();
                if (status.Error)
                {
                    await msg.ModifyAsync(m => m.Content = $"{id} error: {status.ErrorMessage}");
                    return;
                }

                if (status.IsUpdating)
                {
                    await msg.ModifyAsync(m => m.Content = $"{id} is updating. Check last update log later.");
                    return;
                }

                if (status.IsRunning)
                {
                    await msg.ModifyAsync(m => m.Content = $"{id} is running.\n" +
                                                           $"Players: {status.Players}\n" +
                                                           $"Admins: {status.Admins}\n" +
                                                           $"Join now: {status.Address}:{status.Port}");
                    return;
                }

                await msg.ModifyAsync(m => m.Content = $"{id} is offline");
            }
            catch (HttpException e)
            {
                await msg.ModifyAsync(m => m.Content = $"Got exception: ${e.Reason}");
            }
        }

        [Command("status-update")]
        public async void SetStatusUpdate()
        {
            
        }
    }
}
