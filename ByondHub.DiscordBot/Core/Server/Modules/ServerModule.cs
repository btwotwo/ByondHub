using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Preconditions;
using ByondHub.DiscordBot.Core.Server.Services;
using Discord.Commands;
using Discord.Net;

namespace ByondHub.DiscordBot.Core.Server.Modules
{
    [Group("server")]
    [RequireWhitelistedRole]
    public class ServerModule : ModuleBase<SocketCommandContext>
    {
        private readonly ServerService _server;

        public ServerModule(ServerService service)
        {
            _server = service;
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
                    $"Update request for \"{id}\" is finished." +
                    $" Server is up-to-date on branch \"{updateResult.Branch}\" and " +
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
    }
}
