using System.Collections.Generic;
using Newtonsoft.Json;

namespace VkLongPollNet
{
    /// <summary>
    /// Класс для десериализации LongPoll-события "message_reply" 
    /// </summary>
    public class MessageReplyArgs
    {
        [JsonProperty("date")] public int Date { get; set; }

        [JsonProperty("from_id")] public int FromId { get; set; }

        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("out")] public int Out { get; set; }

        [JsonProperty("peer_id")] public int PeerId { get; set; }

        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("conversation_message_id")]
        public int ConversationMessageId { get; set; }

        [JsonProperty("fwd_messages")] public List<object> FwdMessages { get; set; }

        [JsonProperty("important")] public bool Important { get; set; }

        [JsonProperty("random_id")] public int RandomId { get; set; }

        [JsonProperty("attachments")] public List<object> Attachments { get; set; }

        [JsonProperty("admin_author_id")] public int AdminAuthorId { get; set; }

        [JsonProperty("is_hidden")] public bool IsHidden { get; set; }
    }

    /// <summary>
    /// Делегат для обработки события "message_reply"
    /// </summary>
    public delegate void MessageReplyHandler(object sender, MessageReplyArgs messageReplyArgs);
}