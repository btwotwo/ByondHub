using ByondHub.Shared.Web;

namespace ByondHub.Shared.Updates
{
    public class UpdateResult : WebResult
    {
        public string Id { get; set; }
        public bool UpToDate { get; set; }
        public string Output { get; set; }
        public string Branch { get; set; }
        public string CommitHash { get; set; }
        public string CommitMessage { get; set; }
    }
}
