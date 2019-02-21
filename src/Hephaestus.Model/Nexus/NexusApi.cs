using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using Autofac;
using Hephaestus.Model.Core;
using Hephaestus.Model.Core.Interfaces;
using Hephaestus.Model.Nexus.Interfaces;
using Newtonsoft.Json.Linq;
using WebSocket = WebSocketSharp.WebSocket;

namespace Hephaestus.Model.Nexus
{
    public class NexusApi : INexusApi
    {
        private readonly ILogger _logger;

        public delegate void HasLoggedIn();
        public delegate void RequestLimitUpdated(int currentLimit);

        public event HasLoggedIn HasLoggedInEvent;
        public event RequestLimitUpdated RequestLimitUpdatedEvent;

        private HttpClient _httpClient;

        private string _gameName;
        private string _apiKey;

        private bool _isPremium;
        private bool _isLoggedIn;

        public int RemainingDailyRequests { get; set; }

        public NexusApi(IComponentContext components)
        {
            _logger = components.Resolve<ILogger>();
        }

        public async Task New(GameName gameName, string apiKey = "")
        {
            _gameName = gameName.ToString().ToLower();
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.nexusmods.com"),
                Timeout = TimeSpan.FromSeconds(5)
            };

            if (apiKey != string.Empty)
            {
                _apiKey = apiKey;

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("APIKEY", _apiKey);

                // Get the premium status of the account
                var response = await _httpClient.GetAsync("/v1/users/validate.json");
                _isPremium = (bool)JObject.Parse(response.Content.ReadAsStringAsync().Result)["is_premium"];
                _isLoggedIn = true;

                RemainingDailyRequests = Convert.ToInt32(response.Headers.GetValues("X-RL-Daily-Remaining").ToList().First());

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
                var response = _httpClient.GetAsync("/v1/users/validate.json").Result;
                _isPremium = (bool) JObject.Parse(response.Content.ReadAsStringAsync().Result)["is_premium"];
                _isLoggedIn = true;

                RemainingDailyRequests = Convert.ToInt32(response.Headers.GetValues("X-RL-Daily-Remaining").ToList().First());

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
            _logger.Write($"MD5 Query: {md5} \n");

            var response = new HttpResponseMessage();

            try
            {
                response = await _httpClient.GetAsync($"/v1/games/{_gameName}/mods/md5_search/{md5.ToLower()}.json");
                RemainingDailyRequests = Convert.ToInt32(response.Headers.GetValues("X-RL-Daily-Remaining").ToList().First());
            }
            catch (Exception e)
            {
                _logger.Write($"Nexus API request failed: {e.InnerException} \n");

                return null;
            }


            var apiJson = JArray.Parse(response.Content.ReadAsStringAsync().Result);

            _logger.Write($"success.");

            return new GetModsByMd5Result()
            {
                AuthorName = apiJson[0]["mod"]["author"].ToString(),
                ModId = apiJson[0]["mod"]["mod_id"].ToString(),
                FileId = apiJson[0]["file_details"]["file_id"].ToString(),
                ArchiveName = apiJson[0]["file_details"]["file_name"].ToString()
            };
        }
    }
}
