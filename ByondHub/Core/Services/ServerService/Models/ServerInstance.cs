using System.Diagnostics;
using System.Threading.Tasks;
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

        public ServerStatusResult Status { get; private set; }

        public ServerInstance(BuildModel build, string dreamDaemonPath, ServerUpdater updater, string serverAddress)
        {
            Build = build;
            State = new StoppedServerState();
            Status = new ServerStatusResult() { IsRunning = false, IsUpdating = false, Address = serverAddress };
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
            Status.IsRunning = true;
            Status.IsUpdating = false;
            Status.Address = $"{Status.Address}:{port}";
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
            Status.IsRunning = false;
            return new ServerStartStopResult {Id = Build.Id, Message = "Server stopped."};
        }

        public UpdateResult Update(UpdateRequest request)
        {
            try
            {
                State = new UpdatingServerState();
                Status.IsUpdating = true;
                var result = _updater.Update(Build, request.Branch, request.CommitHash);
                return result;
            }
            finally
            {
                State = new StoppedServerState();
                Status.IsUpdating = false;
            }
        }

        public async Task UpdateStatusAsync()
        {
            Status.Players++;
        }
    }
}
