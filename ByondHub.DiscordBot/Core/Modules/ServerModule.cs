using System;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Services;
using Discord;
using Discord.Commands;

namespace ByondHub.DiscordBot.Core.Modules
{
    [RequireUserPermission(GuildPermission.BanMembers)]
    [Group("server")]
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
            string res = await _server.SendStartRequestAsync(id, port);
            await ReplyAsync(res);
        }

        [Command("stop")]
        [Summary("Stops server. Provide id")]
        public async Task StopServer([Summary("Server id")] string id)
        {
            string res = await _server.SendStopRequestAsync(id);
            await ReplyAsync(res);
        }

        [Command("update", RunMode = RunMode.Async)]
        [Summary("Updates server. Provide id.")]

        public async Task UpdateServer([Summary("Server id")] string id,
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
    }
}
