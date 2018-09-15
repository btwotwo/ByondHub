
using LibGit2Sharp;
using System;
using System.Linq;

namespace ByondHub.Core.Utility.Git
{
    public class GitWrapper
    {

        public Repository Repo { get; set; }

        public virtual bool Pull(string username = "", string password = "")
        {
            var pullOptions = new PullOptions
            {
                FetchOptions = new FetchOptions()
            };

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                pullOptions.FetchOptions.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler((url, usernameFromUrl, types) =>
                
                    new UsernamePasswordCredentials()
                    {
                        Username = username,
                        Password = password
                    }
                );
            }

            var signature = new Signature("ByondHub", "ByondHub@mail", DateTimeOffset.Now);
            var result = Commands.Pull(Repo, signature, pullOptions);
            return result.Status == MergeStatus.UpToDate;
        }


        public virtual Branch Checkout(string branchName)
        {
            string remoteBranchName = $"refs/remotes/origin/{branchName}";
            var remoteBranch = Repo.Branches[remoteBranchName] ?? throw new UpdateException($"Branch {branchName} is not found on remote");
            var localBranch = Repo.Branches.SingleOrDefault(x => x.FriendlyName == branchName);

            if (localBranch == null)
            {
                localBranch = Repo.CreateBranch(branchName, remoteBranch.Tip);
                Repo.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
            }

            var checkedOutBranch = Commands.Checkout(Repo, localBranch);
            return checkedOutBranch;
        }

        public virtual void Reset(string commitHash, Branch branch)
        {
            var commit = branch.Commits.FirstOrDefault(x => x.Sha == commitHash) 
                ?? throw new UpdateException($"Commit {commitHash} on {branch.FriendlyName} is not found");

            if (Repo.Head.Tip.Sha == commitHash)
            {
                return;
            }

            Repo.Reset(ResetMode.Hard, commit);
        }

        public virtual void Fetch()
        {
            foreach (var remote in Repo.Network.Remotes)
            {
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(Repo, remote.Name, refSpecs, new FetchOptions
                {
                    Prune = true
                }, "");
            }
        }
    }

    public class UpdateException : Exception
    {
        public UpdateException(string message) : base(message) { }
    }
}
