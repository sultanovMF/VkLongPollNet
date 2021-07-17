using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VkLongPollNet
{
    public class VkMessageInfo
    {
        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("from_id")]
        public int FromId { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("out")]
        public int Out { get; set; }

        [JsonProperty("peer_id")]
        public int PeerId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("conversation_message_id")]
        public int ConversationMessageId { get; set; }

        [JsonProperty("fwd_messages")]
        public List<object> FwdMessages { get; set; }

        [JsonProperty("important")]
        public bool Important { get; set; }

        [JsonProperty("random_id")]
        public int RandomId { get; set; }

        [JsonProperty("attachments")]
        public List<object> Attachments { get; set; }

        [JsonProperty("is_hidden")]
        public bool IsHidden { get; set; }
    }

    public class VkClientInfo
    {
        [JsonProperty("button_actions")]
        public List<string> ButtonActions { get; set; }

        [JsonProperty("keyboard")]
        public bool Keyboard { get; set; }

        [JsonProperty("inline_keyboard")]
        public bool InlineKeyboard { get; set; }

        [JsonProperty("carousel")]
        public bool Carousel { get; set; }

        [JsonProperty("lang_id")]
        public int LangId { get; set; }
    }

    public class MessageNewArgs
    {
        [JsonProperty("message")]
        public VkMessageInfo Message { get; set; }

        [JsonProperty("client_info")]
        public VkClientInfo ClientInfo { get; set; }
    }

    public delegate Task MessageNewHandler(object sender, MessageNewArgs e);
}