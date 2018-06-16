﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ByondHub.Core.Configuration;
using ByondHub.Core.Server.Models.ServerState;
using ByondHub.Shared.Server.Updates;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Server.Services
{
    public class ServerUpdater
    {
        private readonly ILogger _logger;
        private readonly string _dreamMakerPath;
        private readonly ServerInstance _server;

        public ServerUpdater(string dreamMakerPath, ILogger logger, ServerInstance server)
        {
            _logger = logger;
            _dreamMakerPath = dreamMakerPath;
            _server = server;
        }

        public UpdateResult Update(BuildModel build, string branch, string commitHash)
        {
            try
            {
                string repo = build.Path;
                string username = build.RepositoryEmail;
                var result = Pull(repo, username, commitHash, branch);

                if (result.UpToDate)
                {
                    _server.State = new StoppedServerState(_server);
                    return result;
                }

                StartCompilingProcess(build, result);
                return result;
            }
            catch (UpdateException ex)
            {
                _logger.LogError(ex, "Error while updating.");
                _server.State = new StoppedServerState(_server);
                return new UpdateResult {Error = true, ErrorMessage = ex.Message};
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to update.");
                _server.State = new StoppedServerState(_server);
                return new UpdateResult{Error = true, ErrorMessage = ex.Message};
            }
             
        }

        private void StartCompilingProcess(BuildModel build, UpdateResult result)
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
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            var output = new StringBuilder();
            var errorOutput = new StringBuilder();

            dreamMakerProcess.ErrorDataReceived += (sender, args)
                => errorOutput.AppendLine(args.Data);
            dreamMakerProcess.OutputDataReceived += (sender, args)
                => output.AppendLine(args.Data);
            dreamMakerProcess.Exited += (sender, args) =>
            {
                string errors = errorOutput.ToString();
                string log = output.ToString();

                if (errors != "\r\n" && !string.IsNullOrEmpty(errors))
                {
                    log = errors;
                }

                _server.Status.LastBuildLog = log;
                _server.State = new StoppedServerState(_server);
                _logger.LogInformation($"Finished compiling {build.Id}");
                dreamMakerProcess.Dispose();
            };

            dreamMakerProcess.Start();
            dreamMakerProcess.BeginErrorReadLine();
            dreamMakerProcess.BeginOutputReadLine();

        }

        private UpdateResult Pull(string repository, string username, string commitHash, string branchName)
        {
            _logger.LogInformation($"Updating {repository}.");
            using (var repo = new Repository(repository))
            {
                Fetch(repo);
                string remoteBranchName = $"refs/remotes/origin/{branchName ?? "master"}";
                var remoteBranch = repo.Branches[remoteBranchName] ??
                                   throw new UpdateException($"Error. Branch {branchName} is not found on remote.");

                var branch = repo.Branches.SingleOrDefault(x => x.FriendlyName == branchName);

                if (branch == null)
                {
                    branch = repo.CreateBranch(branchName, remoteBranch.Tip);
                    repo.Branches.Update(branch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                }

                if (branch.IsCurrentRepositoryHead)
                {
                    var res = repo.MergeFetchedRefs(new Signature(username, username, DateTimeOffset.Now), null);
                    if (res.Status == MergeStatus.UpToDate && string.IsNullOrEmpty(commitHash))
                    {
                        return new UpdateResult
                        {
                            Branch = repo.Head.FriendlyName,
                            CommitHash = repo.Head.Tip.Sha,
                            CommitMessage = repo.Head.Tip.MessageShort,
                            UpToDate = true
                        };
                    }

                    Reset(commitHash, repo, branch);
                    return new UpdateResult
                    {
                        Branch = repo.Head.FriendlyName,
                        CommitHash = repo.Head.Tip.Sha,
                        CommitMessage = repo.Head.Tip.MessageShort,
                        UpToDate = false
                    };
                }

                Commands.Checkout(repo, branch);
                Fetch(repo); //we're doing second fetch b/c we can't fetch particular branch with libgit2sharp

                repo.MergeFetchedRefs(new Signature(username, username, DateTimeOffset.Now), null);

                if (!string.IsNullOrEmpty(commitHash))
                {
                    Reset(commitHash, repo, branch);
                }

                _logger.LogInformation($"Updated {repository}.");
                return new UpdateResult
                {
                    Branch = repo.Head.FriendlyName,
                    CommitHash = repo.Head.Tip.Sha,
                    CommitMessage = repo.Head.Tip.MessageShort,
                    UpToDate = false
                };
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

        private static bool Reset(string commitHash, IRepository repository, Branch branch)
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