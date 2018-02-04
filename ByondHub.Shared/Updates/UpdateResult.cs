namespace ByondHub.Shared.Updates
{
    public class UpdateResult
    {
        public bool UpToDate { get; set; }
        public bool Error { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }
        public string Branch { get; set; }
        public string CommitHash { get; set; }
        public string CommitMessage { get; set; }
    }
}
