using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ByondHub.Core.Configuration;
using ByondHub.Core.Utility.Byond;
using ByondHub.Shared.Server.Updates;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Server
{
    public interface IServerUpdater
    {
        UpdateResult Update(BuildModel build, string branch, string commitHash);
    }

    public class ServerUpdater : IServerUpdater
    {
        private readonly ILogger<ServerUpdater> _logger;
        private readonly IByondWrapper _byond;

        public ServerUpdater(ILogger<ServerUpdater> logger, IByondWrapper byond)
        {
            _logger = logger;
            _byond = byond;
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
                    return result;
                }
                string buildPath = Path.Combine(build.Path, $"{build.ExecutableName}.dme");
                result.Output = _byond.CompileWorld(buildPath);
                return result;
            }
            catch (UpdateException ex)
            {
                _logger.LogError(ex, "Error while updating.");
                return new UpdateResult {Error = true, ErrorMessage = ex.Message};
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to update.");
                return new UpdateResult {Error = true, ErrorMessage = ex.Message};
            }

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
