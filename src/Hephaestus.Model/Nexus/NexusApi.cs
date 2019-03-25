using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using Autofac;
using Hephaestus.Model.Core;
using Hephaestus.Model.Core.Interfaces;
using Hephaestus.Model.Nexus.Interfaces;
using Hephaestus.Model.Transcompiler;
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

        public async Task<GetModsByMd5Result> GetModsByMd5(IntermediaryModObject mod)
        {
            var md5 = mod.Md5;
            var response = new HttpResponseMessage();

            _logger.Write($"MD5 Query: {md5} \n");

            try
            {
                response = await _httpClient.GetAsync($"/v1/games/{_gameName}/mods/md5_search/{md5.ToLower()}.json");
                RemainingDailyRequests = Convert.ToInt32(response.Headers.GetValues("X-RL-Daily-Remaining").ToList().First());

                var apiJson = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                var possibleJsonObjects = apiJson.Where(x => (bool)x["mod"]["available"] == true).Where(x => (int)x["file_details"]["category_id"] != 6);
                var jsonObject = possibleJsonObjects.First();

                if (possibleJsonObjects.Count() > 1)
                {
                    // We need to step through more advanced algorithms
                    _logger.Write("More than one possible json object AFTER filter. Attempting to fix ComputeLevenshtein()");

                    jsonObject = possibleJsonObjects.Where(x => x["file_details"]["file_name"].ToString() == mod.TrueArchiveName).First();
                }

                if (!possibleJsonObjects.Any())
                {
                    _logger.Write($"No valid json objects for md5: {md5}. Report this to the modpack author, this mod needs to be updated/removed");

                    return null;
                }   

                _logger.Write($"success.\n");

                return new GetModsByMd5Result()
                {
                    AuthorName = jsonObject["mod"]["author"].ToString(),
                    ModId = jsonObject["mod"]["mod_id"].ToString(),
                    FileId = jsonObject["file_details"]["file_id"].ToString(),
                    ArchiveName = jsonObject["file_details"]["file_name"].ToString(),
                    Version = jsonObject["file_details"]["version"].ToString(),
                    NexusFileName = jsonObject["file_details"]["name"].ToString()
                };
            }

            catch (Exception e)
            {
                _logger.Write($"Nexus API request failed: {e.InnerException} \n");

                return null;
            }
        }

        private int ComputeLevenshtein(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
