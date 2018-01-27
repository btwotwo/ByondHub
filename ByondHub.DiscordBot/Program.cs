using System;

namespace ByondHub.DiscordBot
{
    public class Program
    {
        public static void Main(string[] args) => Startup.RunAsync().GetAwaiter().GetResult();
    }
}
