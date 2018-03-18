namespace ByondHub.Shared.Web
{
    public abstract class WebResult
    {
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }

        public string Message { get; set; }
    }
}
