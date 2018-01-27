using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Globals;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ByondHub.DiscordBot.Core.Services
{
    public class ServerService
    {
        private readonly HttpClient _http;
        private readonly string _secret;
        
        public ServerService(IConfiguration config)
        {
            _secret = config["Bot:Backend:SecretCode"];
            var uriBuilder = new UriBuilder("http", config["Bot:Backend:Host"], int.Parse(config["Bot:Backend:Port"]));
            _http = new HttpClient {BaseAddress = uriBuilder.Uri};
        }

        public async Task<string> SendStartRequestAsync(string serverId, int port)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("port", port.ToString()),
                new KeyValuePair<string, string>("secret", _secret) 
            });

            var responseMessage = await _http.PostAsync($"{ApiEndpoints.ServerStart}/{serverId}", content);
            string resultJson = await responseMessage.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(resultJson))
            {
                throw new Exception($"Got {responseMessage.StatusCode} code.");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(resultJson);
            return responseMessage.IsSuccessStatusCode ? result.message : result.error;
        }


        public async Task<string> SendStopRequestAsync(string serverId)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _secret) 
            });
            var responseMessage = await _http.PostAsync($"{ApiEndpoints.ServerStop}/{serverId}", content);
            string resultJson = await responseMessage.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(resultJson))
            {
                throw new Exception($"Got {responseMessage.StatusCode} code.");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(resultJson);
            return responseMessage.IsSuccessStatusCode ? result.message : result.error;
        }
    }
}
