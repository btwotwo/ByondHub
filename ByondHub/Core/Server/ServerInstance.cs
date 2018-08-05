using System;
using System.IO;
using System.Threading.Tasks;
using ByondHub.Core.Configuration;
using ByondHub.Core.Server.ServerState;
using ByondHub.Core.Utility.Byond;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ByondHub.Core.Server
{
    public class ServerInstance
    {
        private DateTime _playersUpdatedTimestamp;
        private IDreamDaemonProcess _dreamDaemonProcess;

        private readonly IServerUpdater _updater;
        private readonly ILogger _logger;
        private readonly IByondWrapper _byond;

        public ServerStateAbstract State { get; set; }
        public BuildModel Build { get; }
        public ServerStatusResult Status { get; }

        public ServerInstance(BuildModel build, IServerUpdater updater, IByondWrapper byond, IOptions<Config> config, ILogger logger)
        {
            string serverAddress = config.Value.Hub.Address;
            Build = build;
            Status = new ServerStatusResult() { IsRunning = false, IsUpdating = false, Address = serverAddress, Id = build.Id};
            State = new StoppedServerState(this);
            _byond = byond;
            _logger = logger;
            _playersUpdatedTimestamp = DateTime.Now;
            _updater = updater;
        }

        public ServerStartStopResult Start(ushort port)
        {
            return State.Start(port, StartInternal);
        }

        public ServerStartStopResult Stop()
        {
            return State.Stop(StopInternal);
        }

        public UpdateResult Update(UpdateRequest request)
        {
            return State.Update(request, UpdateInternal);
        }

        public async Task<ServerStatusResult> GetStatusAsync()
        {
            await State.UpdatePlayersAsync(UpdatePlayersInternalAsync);
            return Status;
        }
        private ServerStartStopResult StopInternal()
        {
            _dreamDaemonProcess.UnexpectedExit -= HandleUnexpectedExit;
            _dreamDaemonProcess.Kill();
            return new ServerStartStopResult { Id = Build.Id, Message = "Server stopped." };
        }

        private UpdateResult UpdateInternal(UpdateRequest request)
        {
            var result = _updater.Update(Build, request.Branch, request.CommitHash);
            Status.LastBuildLog = result.Output;
            return result;
        }

        private async Task UpdatePlayersInternalAsync()
        {
            if (DateTime.Now - _playersUpdatedTimestamp >= TimeSpan.FromSeconds(30))
            {
                string response = await ByondTopic.GetDataAsync("127.0.0.1", Status.Port.ToString(), "status");

                if (response == null)
                {
                    return;
                }

                var parsedResponse = QueryHelpers.ParseQuery(response);

                Status.Players = int.Parse(parsedResponse["players"]);
                Status.Admins = int.Parse(parsedResponse["admins"]);

                _playersUpdatedTimestamp = DateTime.Now;
            }
        }

        private ServerStartStopResult StartInternal(ushort port)
        {
            try
            {
                string buildPath = $"{Build.Path}/{Build.ExecutableName}.dmb";
                var startParams = new DreamDaemonArguments(buildPath, port);
                var dreamDaemon = _byond.StartDreamDaemon(startParams);
                _dreamDaemonProcess = dreamDaemon;
                _dreamDaemonProcess.UnexpectedExit += HandleUnexpectedExit;
                Status.Port = port;

                return new ServerStartStopResult()
                {
                    Message = "Started server.",
                    Id = Build.Id,
                    Port = port
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting server. Id: {Build.Id}");
                return new ServerStartStopResult()
                {
                    Error = true,
                    ErrorMessage = $"Failed to start server. Exception: \"{ex.Message}\"",
                    Id = Build.Id
                };
            }
            
        }

        private void HandleUnexpectedExit(object sender, int exitCode)
        {
            State = new StoppedServerState(this);
            State.UpdateStatus();
            _logger.LogWarning($"Server with id ${Build.Id} unexpectedly stopped. Exit code: ${exitCode}");
        }
    }
}
