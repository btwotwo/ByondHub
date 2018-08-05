using System;
using System.Diagnostics;
using System.Text;

namespace ByondHub.Core.Utility.Byond
{
    public interface IDreamDaemonProcess
    {
        event EventHandler<int> UnexpectedExit;
        void Create(DreamDaemonArguments args);
        void Kill();
    }

    public class DreamDaemonProcess : IDreamDaemonProcess
    {
        private Process _process;
        private readonly string _dreamDaemonPath;
        public event EventHandler<int> UnexpectedExit;

        public DreamDaemonProcess(string dreamDaemonPath)
        {
            _dreamDaemonPath = dreamDaemonPath;
        }

        public void Create(DreamDaemonArguments args)
        {
            var argsBuilder = new StringBuilder($"{args.ExecutablePath} {args.Port}");
            if (args.Safe)
            {
                argsBuilder.Append("-safe ");
            }

            if (args.Invisible)
            {
                argsBuilder.Append("-invisible ");
            }

            if (args.LogSelf)
            {
                argsBuilder.Append("-logself ");
            }
            var info = new ProcessStartInfo(_dreamDaemonPath)
            {
                Arguments = argsBuilder.ToString()
            };

            var dreamDaemonProcess = new Process()
            {
                StartInfo = info,
                EnableRaisingEvents = true,
            };
            dreamDaemonProcess.Exited += DreamDaemonProcessOnExited;
            dreamDaemonProcess.Start();
            _process = dreamDaemonProcess;
        }

        private void DreamDaemonProcessOnExited(object sender, EventArgs eventArgs)
        {
            UnexpectedExit?.Invoke(this, _process.ExitCode);
        }

        public void Kill()
        {
            _process.Exited -= DreamDaemonProcessOnExited;
            _process.Kill();
        }

    }
}