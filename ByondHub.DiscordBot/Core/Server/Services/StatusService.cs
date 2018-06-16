using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Models;
using ByondHub.Shared.Server;
using Discord;
using Discord.WebSocket;

namespace ByondHub.DiscordBot.Core.Server.Services
{
    public class StatusService
    {
        private readonly IServerRequester _requester;
        private readonly List<Status> _statuses;

        public StatusService(IServerRequester requester)
        {
            _requester = requester;
            _statuses = new List<Status>();
        }

        public async Task<ServerStatusResult> StartUpdating(string id, ISocketMessageChannel updateChannel)
        {
            var status = _statuses.FirstOrDefault(x => x.ServerId == id);

            if (status != null)
            {
                return new ServerStatusResult() {ErrorMessage = "Server status is already updating.", Error = true};
            }

            var serverStatus = await _requester.SendStatusRequestAsync(id);

            if (serverStatus.Error)
            {
                return serverStatus;
            }

            return new ServerStatusResult();
        }
    }
}
