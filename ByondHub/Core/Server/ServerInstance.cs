using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ByondHub.Core.Configuration;
using ByondHub.Core.Server.Models.ServerState;
using ByondHub.Core.Server.Services;
using ByondHub.Core.Utility;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ByondHub.Core.Server
{
    public class ServerInstance
    {
        private Process _process;
        private readonly string _dreamDaemonPath;
        private readonly ServerUpdater _updater;
        private readonly ILogger _logger;

        public ServerStateAbstract State { get; set; }
        public BuildModel Build { get; }
        public bool IsRunning => !_process.HasExited;

        public ServerStatusResult Status { get; }
        private DateTime _playersUpdatedTimestamp;

        public ServerInstance(BuildModel build, IOptions<Config> options, string serverAddress, ILogger logger)
        {
            var config = options.Value;
            Build = build;
            Status = new ServerStatusResult() { IsRunning = false, IsUpdating = false, Address = serverAddress };
            State = new StoppedServerState(this);
            _dreamDaemonPath = config.Hub.DreamDaemonPath;
            _updater = new ServerUpdater(config.Hub.DreamMakerPath, logger);
            _logger = logger;
            _playersUpdatedTimestamp = DateTime.Now;
        }

        public ServerStartStopResult Start(int port)
        {
            try
            {
                var info = new ProcessStartInfo($"{_dreamDaemonPath}")
                {
                    Arguments = $"{Build.Path}/{Build.ExecutableName}.dmb {port} -safe -invisible -logself"
                };
                _process = new Process
                {
                    StartInfo = info,
                    EnableRaisingEvents = true
                };
                _process.Exited += HandleUnexpectedExit;
                _process.Start();

                if (_process.HasExited)
                {
                    return new ServerStartStopResult
                    {
                        Error = true,
                        ErrorMessage = "Failed to start server.",
                        Id = Build.Id
                    };
                }

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
                    ErrorMessage = $"Failed to start server. Exception: {ex.Message}",
                    Id = Build.Id
                };
            }
            
        }


        public ServerStartStopResult Stop()
        {
            _process.Exited -= HandleUnexpectedExit;
            _process.Kill();
            _process.Dispose();
            return new ServerStartStopResult {Id = Build.Id, Message = "Server stopped."};
        }

        public UpdateResult Update(UpdateRequest request)
        {
            var result = _updater.Update(Build, request.Branch, request.CommitHash);
            Status.LastBuildLog = result.Output;
            return result;
        }

        public async Task UpdatePlayersAsync()
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

        private void HandleUnexpectedExit(object sender, EventArgs args)
        {
            State = new StoppedServerState(this);
            State.UpdateStatus();
            _logger.LogWarning($"Server with id ${Build.Id} unexpectedly stopped. Exit code: ${_process.ExitCode}");
            _process.Dispose();
        }
    }
}
