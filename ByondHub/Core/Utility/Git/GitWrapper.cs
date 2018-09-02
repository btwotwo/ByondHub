
using LibGit2Sharp;
using PullOptions = ByondHub.Core.Utility.Git.Models.PullOptions;

namespace ByondHub.Core.Utility.Git
{
    public interface IGitWrapper
    {
        void Pull(Repository repo, PullOptions options);
    }
    public class GitWrapper
    {
    }
}
