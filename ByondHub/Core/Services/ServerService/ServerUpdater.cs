using System;
using System.Diagnostics;
using System.Text;
using ByondHub.Core.Configuration;
using ByondHub.Core.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Services.ServerService
{
    public class ServerUpdater
    {
        private readonly ILogger _logger;
        private readonly string _dreamMakerPath;

        public ServerUpdater(IConfiguration config, ILogger logger)
        {
            _logger = logger;
            _dreamMakerPath = config["Hub:DreamMakerPath"];
        }

        public (string output, string errors) Update(BuildModel build)
        {
            string repo = build.Path;
            string username = build.RepositoryEmail;
            string password = build.RepositoryPassword;
            if (string.IsNullOrEmpty(repo)) throw new Exception("Please specify repository path.");

            bool upToDate = Pull(repo, username, password);
            return !upToDate ? Compile(build) : ("Server is up to date.", "");
        }

        private (string output, string errors) Compile(BuildModel build)
        {
            _logger.LogInformation($"Starting to compile {build.Id}");
            var startInfo = new ProcessStartInfo(_dreamMakerPath)
            {
                Arguments = $"{build.Path}/{build.ExecutableName}.dme",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var dreamMakerProcess = new Process
            {
                StartInfo = startInfo
            };

            var output = new StringBuilder();
            var errorOutput = new StringBuilder();

            dreamMakerProcess.ErrorDataReceived += (sender, args)
                => errorOutput.AppendLine(args.Data);
            dreamMakerProcess.OutputDataReceived += (sender, args)
                => output.AppendLine(args.Data);

            dreamMakerProcess.Start();
            dreamMakerProcess.BeginErrorReadLine();
            dreamMakerProcess.BeginOutputReadLine();
            dreamMakerProcess.WaitForExit();

            _logger.LogInformation($"Finished compiling {build.Id}");

            return (output.ToString(), errorOutput.ToString());
        }

        private bool Pull(string repository, string username, string password)
        {
            _logger.LogInformation($"Pulling {repository}.");
            using (var repo = new Repository(repository))
            {
                var options = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials
                            {
                                Username = username,
                                Password = password
                            }
                    }
                };
                var result = Commands.Pull(repo, new Signature(username, username, new DateTimeOffset(DateTime.Now)), options);

                _logger.LogInformation($"Pulled {repository}.");
                return result.Status == MergeStatus.UpToDate;
            }
        }
    }
}
