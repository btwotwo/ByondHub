using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ByondHub.Core.Server.Services
{
    public class ServerLogs
    {
        private IConfiguration _config;

        public ServerLogs(IConfiguration config)
        {
            _config = config;
        }

        public FileStream GetSelflog(string serverName)
        {
            throw new NotImplementedException();
            //var server = _config;
        }

        public FileStream GetWorldLog(string serverId)
        {
            throw new NotImplementedException();
        }
    }
}
