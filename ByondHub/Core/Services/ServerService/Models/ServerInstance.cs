using System.Diagnostics;
using ByondHub.Core.Configuration;
using ByondHub.Core.Services.ServerService.ServerState;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;

namespace ByondHub.Core.Services.ServerService.Models
{
    public class ServerInstance
    {
        private Process _process;
        private readonly string _dreamDaemonPath;
        private readonly ServerUpdater _updater;

        public IServerState State { get; set; }
        public BuildModel Build { get; }
        public bool IsRunning => !_process.HasExited;

        public ServerInstance(BuildModel build, string dreamDaemonPath, ServerUpdater updater)
        {
            Build = build;
            State = new StoppedServerState();
            _dreamDaemonPath = dreamDaemonPath;
            _updater = updater;
        }

        public ServerStartStopResult Start(int port)
        {
            var info = new ProcessStartInfo($"{_dreamDaemonPath}")
            {
                Arguments = $"{Build.Path}/{Build.ExecutableName}.dmb {port} -safe -invisible -logself",
                CreateNoWindow = true,
                UseShellExecute = true
            };
            _process = new Process {StartInfo = info};
            bool started = _process.Start();
            if (!started)
            {
                State = new StoppedServerState();
                return new ServerStartStopResult
                {
                    Error = true,
                    ErrorMessage = "Failed to start server.",
                    Id = Build.Id
                };
            }
            _process.Exited += (sender, args) => State = new StoppedServerState();
            State = new StartedServerState();
            return new ServerStartStopResult()
            {
                Message = "Started server.",
                Id = Build.Id,
                Port = port
            };
        }

        public ServerStartStopResult Stop()
        {
            _process.Kill();
            _process.Dispose();
            State = new StoppedServerState();
            return new ServerStartStopResult {Id = Build.Id, Message = "Server stopped."};
        }

        public UpdateResult Update(UpdateRequest request)
        {
            try
            {
                State = new UpdatingServerState();
                var result = _updater.Update(Build, request.Branch, request.CommitHash);
                return result;
            }
            finally
            {
                State = new StoppedServerState();
            }
        }
    }
}
