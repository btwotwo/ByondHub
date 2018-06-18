using System;
using System.Collections.Generic;
using ByondHub.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ByondHub.Core.Server.Services
{
    public class ServerStore
    {
        private readonly Dictionary<string, Models.Server> _servers;

        public ServerStore(IOptions<Config> options, IServiceProvider services)
        {
            var config = options.Value;
            _servers = new Dictionary<string, Models.Server>();
            var builds = config.Hub.Builds;
            foreach (var build in builds)
            {
                _servers.Add(build.Id,
                    new Models.Server(
                        new ServerInstance(build, options,
                            config.Hub.Address, services.GetService<ILogger<ServerInstance>>()
                        )
                    )
                );
            }
        }

        public Models.Server GetServer(string id)
        {
            return _servers.GetValueOrDefault(id);
        }
    }
}
