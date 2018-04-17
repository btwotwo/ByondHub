using System;
using System.Collections.Generic;
using System.Text;

namespace ByondHub.Shared.Web
{
    public class ServerStatusResult : WebResult
    {
        public string Address { get; set; }
        public bool IsRunning { get; set; }
        public bool IsUpdating { get; set; }
        public int Admins { get; set; }
        public int Players { get; set; }
    }
}
