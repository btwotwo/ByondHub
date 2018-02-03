namespace ByondHub.Shared.Core
{
    public abstract class AuthenticatedRequest
    {
        public string SecretKey { get; set; }
    }
}
