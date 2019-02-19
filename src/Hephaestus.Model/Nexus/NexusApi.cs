using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using Hephaestus.Model.Core;
using Hephaestus.Model.Nexus.Interfaces;
using Newtonsoft.Json.Linq;
using WebSocket = WebSocketSharp.WebSocket;

namespace Hephaestus.Model.Nexus
{
    public class NexusApi : INexusApi
    {
        public delegate void HasLoggedIn();

        public event HasLoggedIn HasLoggedInEvent;

        private HttpClient _httpClient;

        private string _gameName;
        private string _apiKey;

        private bool _isPremium;
        private bool _isLoggedIn;

        public async Task New(GameName gameName, string apiKey = "")
        {
            _gameName = gameName.ToString().ToLower();
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.nexusmods.com")
            };

            if (apiKey != string.Empty)
            {
                _apiKey = apiKey;

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("APIKEY", _apiKey);

                // Get the premium status of the account
                var response = await _httpClient.GetStringAsync("/v1/users/validate.json");
                _isPremium = (bool)JObject.Parse(response)["is_premium"];
                _isLoggedIn = true;

                HasLoggedInEvent.Invoke();

                return;
            }

            // If there is no API key passed through, generate a new one.
            var guid = Guid.NewGuid();
            var websocket = new WebSocket("wss://sso.nexusmods.com")
            {
                SslConfiguration = {EnabledSslProtocols = SslProtocols.Tls12}
            };

            websocket.OnMessage += (sender, args) =>
            {
                if (args == null || string.IsNullOrEmpty(args.Data)) return;

                _apiKey = args.Data;

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("APIKEY", _apiKey);

                // Get the premium status of the account
                var response = _httpClient.GetStringAsync("/v1/users/validate.json").Result;
                _isPremium = (bool) JObject.Parse(response)["is_premium"];
                _isLoggedIn = true;

                HasLoggedInEvent.Invoke();
            };

            await Task.Factory.StartNew(() =>
            {
                websocket.Connect();
                websocket.Send("{\"id\": \"" + guid + "\", \"appid\": \"The Automaton Framework\"}");
            });

            Process.Start($"https://www.nexusmods.com/sso?id={guid}");
        }

        public string ApiKey()
        {
            return _apiKey;
        }

        public async Task<GetModsByMd5Result> GetModsByMd5(string md5)
        {
            var response = await _httpClient.GetStringAsync($"/v1/games/{_gameName}/mods/md5_search/{md5.ToLower()}.json");
            var apiJson = JArray.Parse(response);

            return new GetModsByMd5Result()
            {
                AuthorName = apiJson[0]["mod"]["author"].ToString(),
                ModId = apiJson[0]["mod"]["mod_id"].ToString(),
                FileId = apiJson[0]["file_details"]["file_id"].ToString()
            };
        }
    }
}
