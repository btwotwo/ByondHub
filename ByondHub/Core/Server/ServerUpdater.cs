using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ByondHub.Core.Configuration;
using ByondHub.Core.Utility.Byond;
using ByondHub.Core.Utility.Git;
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
        private readonly GitWrapper _git;

        public ServerUpdater(ILogger<ServerUpdater> logger, IByondWrapper byond, GitWrapper git)
        {
            _logger = logger;
            _byond = byond;
            _git = git;
        }

        public UpdateResult Update(BuildModel build, string branch, string commitHash)
        {
            try
            {
                var result = new UpdateResult()
                {
                    Id = build.Id
                };

                using (var repo = new Repository(build.Path))
                {
                    _git.Repo = repo;
                    _git.Fetch();
                    var checkoutBranch = _git.Checkout(branch);
                    _git.Pull();

                    if (!string.IsNullOrEmpty(commitHash))
                    {
                        _git.Reset(commitHash, checkoutBranch);
                    }

                    result.Branch = branch;
                    result.CommitMessage = checkoutBranch.Tip.MessageShort;
                    result.CommitHash = checkoutBranch.Tip.Sha;
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
    }
}
