using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Models;
using ByondHub.Shared.Server;
using Discord;
using Microsoft.Extensions.Logging;

namespace ByondHub.DiscordBot.Core.Server.Services
{
    public class StatusService
    {
        private readonly IServerRequester _requester;
        private readonly Dictionary<string, Status> _statuses;
        private readonly Timer _statusUpdateTimer;
        private readonly ILogger _logger;

        public StatusService(IServerRequester requester, ILogger logger)
        {
            _requester = requester;
            _statuses = new Dictionary<string, Status>();
            _statusUpdateTimer = new Timer(UpdateStatusesAsync, _statuses, 0, Timeout.Infinite);
            _logger = logger;
        }

        public async Task<ServerStatusResult> StartUpdatingAsync(string id, IMessageChannel updateChannel)
        {
            bool updating = _statuses.ContainsKey(id);

            if (updating)
            {
                return new ServerStatusResult() {ErrorMessage = "Server status is already updating.", Error = true};
            }

            var serverStatus = await _requester.SendStatusRequestAsync(id);

            if (serverStatus.Error)
            {
                return serverStatus;
            }
            var msg = await updateChannel.SendMessageAsync("Updating server status...");
            var status = new Status()
            {
                LastUpdateTime = DateTime.Now,
                ServerId = id,
                StatusResult = serverStatus,
                Message = msg
            };
            _statuses[id] = status;
            return new ServerStatusResult {Message = "Started server status updating."};
        }

        public async Task<ServerStatusResult> StopUpdatingAsync(string id)
        {
            var status = _statuses.GetValueOrDefault(id);

            if (status == null)
            {
                return new ServerStatusResult()
                {
                    Error = true,
                    ErrorMessage = $"{id.ToUpper()} status is not updating. "
                }; 
            }

            _statuses.Remove(id);
            await status.StopAsync();
            return new ServerStatusResult() {Message = "Stopped status updating."};
        }

        private async void UpdateStatusesAsync(object state)
        {
            try
            {
                if (!(state is Dictionary<string, Status> statuses))
                {
                    return;
                }
                var tasks = statuses.Select(x => UpdateServerStatusAsync(x.Value, x.Key));
                await Task.WhenAll(tasks.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating server status: {e}");
            }
            finally
            {
                _statusUpdateTimer.Change(5000, Timeout.Infinite);
            }
        }

        private async Task UpdateServerStatusAsync(Status status, string id)
        {
            try
            {
                var newStatus = await _requester.SendStatusRequestAsync(id);
                await status.UpdateAsync(newStatus);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating server status. Id: {status.ServerId}. {e}");
            }
        }

    }

}
