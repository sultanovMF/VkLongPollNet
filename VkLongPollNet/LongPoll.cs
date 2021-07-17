using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Model;

namespace VkLongPollNet
{
    internal class LongPollEventUpdate
    {
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("object")] public JObject Object { get; set; }

        [JsonPropertyName("group_id")] public int GroupId { get; set; }

        [JsonPropertyName("event_id")] public string EventId { get; set; }
    }
    internal class LongPollEventRoot
    {
        [JsonPropertyName("ts")] public string Ts { get; set; }

        [JsonPropertyName("updates")] public List<LongPollEventUpdate> Updates { get; set; }
    }

    public class LongPoll
    {
        public event MessageNewHandler MessageNew;
        
        private LongPollServerResponse _longPollServerResponse;

        public LongPoll(LongPollServerResponse longPollServerResponse)
        {
            this._longPollServerResponse = longPollServerResponse;
        }

        public async Task Listen(CancellationToken stoppingToken, int wait = 20, int mode = 2, int version = 2)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                LongPollEventRoot lpEvent = await GetEventFromLongPoll(wait, mode, version);
                _longPollServerResponse.Ts = lpEvent.Ts;
                foreach (var update in lpEvent.Updates)
                {
                    switch (update.Type)
                    {
                        case "message_new":
                            MessageNewArgs messageNewArgs = update.Object.ToObject<MessageNewArgs>();
                            MessageNew?.Invoke(this, messageNewArgs);
                            break;
                        case "message_reply":
                            break;
                        case "message_edit":
                            break;
                        case "message_allow":
                            break;
                        case "message_deny":
                            break;
                        case "message_typing_state":
                            break;
                        case "message_event":
                            break;
                        
                    }
                }
            }
        }

        private async Task<LongPollEventRoot> GetEventFromLongPoll(int wait = 20, int mode = 2, int version = 2)
        {
            using HttpClient httpClient = new HttpClient();
            UriBuilder uriBuilder = new UriBuilder(_longPollServerResponse.Server)
            {
                Query =
                    $"act=a_check&key={_longPollServerResponse.Key}&ts={_longPollServerResponse.Ts}&wait={wait}&mode={mode}&version={version}"
            };
            var response = await httpClient.GetAsync(uriBuilder.Uri);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<LongPollEventRoot>(await response.Content.ReadAsStringAsync());
        }
    }
}