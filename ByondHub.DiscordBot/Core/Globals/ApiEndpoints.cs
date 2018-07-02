namespace ByondHub.DiscordBot.Core.Globals
{
    public static class ApiEndpoints
    {
        public static string ServerStart (string id, int port) => $"/api/server/{id}/start?port={port}";
        public static string ServerStop (string id) => $"/api/server/{id}/stop";
        public static string ServerUpdate (string id) => $"/api/server/{id}/update";
        public static string WorldLog (string id) => $"/api/server/{id}/worldLog";
        public static string Status (string id) => $"/api/server/{id}/";
    }
}
