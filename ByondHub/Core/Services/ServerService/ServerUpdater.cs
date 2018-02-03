using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ByondHub.Core.Configuration;
using ByondHub.Shared.Updates;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
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

        public UpdateResult Update(BuildModel build, string branch, string commitHash)
        {
            try
            {
                string repo = build.Path;
                string username = build.RepositoryEmail;
                bool upToDate = Pull(repo, username, commitHash, branch);

                return upToDate ? new UpdateResult {Ouput = "Server is up-to-date."} : Compile(build);
            }
            catch (UpdateException ex)
            {
                return new UpdateResult {Error = true, ErrorMessage = ex.Message};
            }
             
        }

        private UpdateResult Compile(BuildModel build)
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

            return new UpdateResult {ErrorMessage = errorOutput.ToString(), Ouput = output.ToString()};
        }

        private bool Pull(string repository, string username, string commitHash, string branchName)
        {
            _logger.LogInformation($"Updating {repository}.");
            using (var repo = new Repository(repository))
            {
                Fetch(repo);
                var branch = repo.Branches.SingleOrDefault(x => x.FriendlyName == (branchName ?? "master")) ?? throw new UpdateException($"Branch \"{branchName}\" is not found.");

                if (branch.IsCurrentRepositoryHead)
                {
                    return Reset(commitHash, repo, branch);
                }

                Commands.Checkout(repo, branch);
                Fetch(repo); //we're doing second fetch b/c we can't fetch particular branch with libgit2sharp

                repo.MergeFetchedRefs(new Signature(username, username, DateTimeOffset.Now), null);

                if (!string.IsNullOrEmpty(commitHash))
                {
                    Reset(commitHash, repo, branch);
                }

                _logger.LogInformation($"Updated {repository}.");
                return false;
            }


        }

        private static void Fetch(Repository repo)
        {
            foreach (var remote in repo.Network.Remotes)
            {
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, remote.Name, refSpecs, new FetchOptions()
                {
                    Prune = true
                }, "");
            }
        }

        private static bool Reset(string commitHash, Repository repository, Branch branch)
        {
            if (string.IsNullOrEmpty(commitHash))
            {
                return true;
            }

            var commit = branch.Commits.FirstOrDefault(x => x.Sha == commitHash) ??
                         throw new UpdateException($"Commit {commitHash} on {branch.FriendlyName} is not found");

            if (repository.Head.Tip.Sha == commit.Sha)
            {
                return true;
            }

            repository.Reset(ResetMode.Hard, commit);
            return false;
        }
    }

    public class UpdateException : Exception
    {
        public UpdateException(string message) : base(message) { }
    }
}
