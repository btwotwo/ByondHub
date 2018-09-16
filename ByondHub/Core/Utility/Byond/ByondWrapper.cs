using System;
using System.Diagnostics;
using System.Text;
using ByondHub.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ByondHub.Core.Utility.Byond
{
    public interface IByondWrapper
    {
        IDreamDaemonProcess StartDreamDaemon(DreamDaemonArguments args);
        string CompileWorld(string dmePath);
    }

    public class ByondWrapper : IByondWrapper
    {
        private readonly string _dreamDaemonPath;
        private readonly string _dreamMakerPath;
        private readonly ILogger<ByondWrapper> _logger;

        public ByondWrapper(IOptions<Config> config, ILogger<ByondWrapper> logger)
        {
            _dreamDaemonPath = config.Value.Hub.DreamDaemonPath;
            _dreamMakerPath = config.Value.Hub.DreamMakerPath;
            _logger = logger;
        }

        public IDreamDaemonProcess StartDreamDaemon(DreamDaemonArguments args)
        {
            var process = new DreamDaemonProcess(_dreamDaemonPath);
            process.Create(args);
            return process;
        }

        public string CompileWorld(string dmePath)
        {
            _logger.LogInformation($"Compiling world in {dmePath}");
            var startInfo = new ProcessStartInfo(_dreamMakerPath)
            {
                Arguments = dmePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var dreamMakerProcess = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            var output = new StringBuilder();
            var errorOutput = new StringBuilder();

            dreamMakerProcess.ErrorDataReceived += (sender, args) => errorOutput.AppendLine(args.Data);
            dreamMakerProcess.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);

            dreamMakerProcess.Start();
            dreamMakerProcess.BeginErrorReadLine();
            dreamMakerProcess.BeginOutputReadLine();

            dreamMakerProcess.WaitForExit();
            _logger.LogInformation($"Finished compiling world in {dmePath}");
            string errors = errorOutput.ToString();
            string log = output.ToString();

            return string.IsNullOrWhiteSpace(errors) ? log : errors;
        }
    }
}
