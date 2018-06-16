using ByondHub.Shared.Core;

namespace ByondHub.Shared.Server
{
    public class ServerStatusResult : WebResult
    {
        public string Address { get; set; }
        public bool IsRunning { get; set; }
        public bool IsUpdating { get; set; }
        public int Admins { get; set; }
        public int Players { get; set; }
        public int Port { get; set; }
        public string LastBuildLog { get; set; }

        public void SetStarted()
        {
            IsRunning = true;
            IsUpdating = false;
            Players = 0;
            Admins = 0;
        }

        public void SetStopped()
        {
            IsRunning = false;
            IsUpdating = false;
            Players = 0;
            Admins = 0;
        }

        public void SetUpdating()
        {
            LastBuildLog = "";
            IsRunning = false;
            IsUpdating = false;
            Players = 0;
            Admins = 0;
        }
    }
}
