using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ByondHub.Core.Configuration;
using ByondHub.Core.Services.ServerService.ServerState;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Services.ServerService.Models
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

        public ServerInstance(BuildModel build, string dreamDaemonPath, ServerUpdater updater, string serverAddress, ILogger logger)
        {
            Build = build;
            State = new StoppedServerState(this);
            Status = new ServerStatusResult() { IsRunning = false, IsUpdating = false, Address = serverAddress };
            _dreamDaemonPath = dreamDaemonPath;
            _updater = updater;
            _logger = logger;
        }

        public ServerStartStopResult Start(int port)
        {
            try
            {
                var info = new ProcessStartInfo($"{_dreamDaemonPath}")
                {
                    Arguments = $"{Build.Path}/{Build.ExecutableName}.dmb {port} -safe -invisible -logself",
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                _process = new Process {StartInfo = info};
                _process.Start();
                _process.Exited += HandleUnexpectedExit;

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
            return result;
        }

        public async Task UpdatePlayersAsync()
        {
            Status.Players++;
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
