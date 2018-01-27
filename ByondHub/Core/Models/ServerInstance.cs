using System.Diagnostics;
using ByondHub.Core.Configuration;

namespace ByondHub.Core.Models
{
    public class ServerInstance
    {
        private Process _process;
        private readonly BuildModel _build;
        private readonly int _port;
        private readonly string _dreamDaemonPath;

        public ServerInstance(BuildModel build, string dreamDaemonPath, int port)
        {
            _build = build;
            _port = port;
            _dreamDaemonPath = dreamDaemonPath;
        }

        public bool Start()
        {
            var info = new ProcessStartInfo($"{_dreamDaemonPath}")
            {
                Arguments = $"{_build.Path}/{_build.ExecutableName}.dmb {_port} -safe -invisible -logself",
                CreateNoWindow = true,
                UseShellExecute = true
            };
            _process = new Process {StartInfo = info};
            return _process.Start();
        }

        public void Stop()
        {
            _process.Kill();
            _process.Dispose();
        }
    }
}
