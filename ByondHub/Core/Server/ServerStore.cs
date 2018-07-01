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
        private readonly Dictionary<string, Server> _servers;

        public ServerStore(IOptions<Config> options, IServiceProvider services)
        {
            var config = options.Value;
            _servers = new Dictionary<string, Server>();
            var builds = config.Hub.Builds;
            foreach (var build in builds)
            {
                _servers.Add(build.Id,
                    new Server(
                        new ServerInstance(build, options,
                            config.Hub.Address, services.GetService<ILogger<ServerInstance>>()
                        )
                    )
                );
            }
        }

        public Server GetServer(string id)
        {
            return _servers.GetValueOrDefault(id);
        }
    }
}
