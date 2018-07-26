using System;
using System.Collections.Generic;
using ByondHub.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ByondHub.Core.Server
{
    public class ServerStore
    {
        private readonly Dictionary<string, ServerInstance> _servers;

        public ServerStore(IOptions<Config> options, IServiceProvider services)
        {
            var config = options.Value;
            var builds = config.Hub.Builds;
            _servers = new Dictionary<string, ServerInstance>();

            foreach (var build in builds)
            {
                _servers.Add(build.Id,
                    new ServerInstance(build, options,
                        config.Hub.Address, services.GetService<ILogger<ServerInstance>>()
                    )
                );
            }
        }

        public ServerInstance GetServer(string id)
        {
            return _servers.GetValueOrDefault(id);
        }
    }
}
