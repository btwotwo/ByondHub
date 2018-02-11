using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ByondHub.Core.Services.ServerLog
{
    public class ServerLogService : IServerLogService
    {
        public FileStream GetSaylog(DateTime date)
        {
            throw new NotImplementedException();
        }

        public FileStream GetRuntimeLog(DateTime? date = null)
        {
            throw new NotImplementedException();
        }
    }
}
