using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
