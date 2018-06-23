using System;
using System.Threading.Tasks;
using ByondHub.Shared.Server;
using Discord;

namespace ByondHub.DiscordBot.Core.Models
{
    public class Status
    {
        public IUserMessage Message { get; set; }
        public string ServerId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public ServerStatusResult StatusResult { get; set; }

        public async Task UpdateAsync(ServerStatusResult newStatus)
        {
            StatusResult = newStatus;
            var embed = StatusResult.Error ? BuildErrorEmbed() : BuildStatusEmbed();
            await Message.ModifyAsync(x =>
            {
                x.Content = "";
                x.Embed = embed;
            });
        }

        public async Task StopAsync()
        {
            await Message.DeleteAsync();
        }

        private Embed BuildStatusEmbed()
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle($"{ServerId.ToUpper()} server status.");
            if (StatusResult.IsRunning)
            {
                embedBuilder.AddField("Join now!",
                    $"{StatusResult.Address}:{StatusResult.Port}");
                embedBuilder.AddInlineField("Players", StatusResult.Players);
                embedBuilder.AddInlineField("Admins", StatusResult.Admins);
                
            } else if (StatusResult.IsUpdating)
            {
                embedBuilder.WithDescription("Server is updating!");
            }
            else
            {
                embedBuilder.WithDescription("Server is offline.");
            }

            embedBuilder.WithFooter($"Last update: {DateTime.Now:HH:mm:ss}");
            embedBuilder.WithColor(Color.Orange);
            return embedBuilder.Build();
        }

        private Embed BuildErrorEmbed()
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle($"{ServerId.ToUpper()} server status.");
            embedBuilder.AddInlineField("Error updating status: ", StatusResult.ErrorMessage);
            embedBuilder.WithColor(Color.DarkRed);
            return embedBuilder.Build();
        }
    }
}
