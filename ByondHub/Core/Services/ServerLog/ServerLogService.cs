
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ByondHub.Core.Services.ServerLog
{
    public class ServerLogService
    {
        private IConfiguration _config;

        public ServerLogService(IConfiguration config)
        {
            _config = config;
        }

        public FileStream GetSelflog(string serverName)
        {
            throw new NotImplementedException();
            //var server = _config;
        }

        public FileStream GetRuntimeLog(DateTime? date = null)
        {
            throw new NotImplementedException();
        }
    }
}
