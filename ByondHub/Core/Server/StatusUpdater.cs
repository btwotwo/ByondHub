using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByondHub.Core.Server
{
    public interface IStatusUpdater
    {
        Status Get(string address, ushort port);
    }

    public class StatusUpdater : IStatusUpdater
    {
    }

    public class Status
    {
        public int Players { get; set; }
        public int Admins { get; set; }
    }
}
