using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace ByondHub.DiscordBot.Core.Preconditions
{
    public class RequireWhitelistedRoleAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = (IConfiguration)services.GetService(typeof(IConfiguration));
            var allowedRoleIds = config.GetSection("Bot:AdministratorRoles").Get<ulong[]>();
            var user = (SocketGuildUser)context.User;
            bool isAllowed = user.Roles.Any(role => allowedRoleIds.Contains(role.Id));

            return isAllowed
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("You do not have permissions to do that."));
        }
    }
}
