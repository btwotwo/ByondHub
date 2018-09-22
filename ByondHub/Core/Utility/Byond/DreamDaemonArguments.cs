namespace ByondHub.Core.Utility.Byond
{
    public class DreamDaemonArguments
    {
        public DreamDaemonArguments(string executablePath, ushort port)
        {
            ExecutablePath = executablePath;
            Port = port;
        }
        public string ExecutablePath { get; set; }
        public ushort Port { get; set; }
        public bool Safe { get; set; } = true;
        public bool Invisible { get; set; } = false;
        public bool LogSelf { get; set; } = true;
    }
}