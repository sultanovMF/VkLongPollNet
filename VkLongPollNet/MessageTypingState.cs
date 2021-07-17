using Newtonsoft.Json;

namespace VkLongPollNet
{
    /// <summary>
    /// Класс для десериализации LongPoll-события "message_typing_state" 
    /// </summary>
    public class MessageTypingStateArgs
    {
        [JsonProperty("state")] public string State { get; set; }

        [JsonProperty("from_id")] public int FromId { get; set; }

        [JsonProperty("to_id")] public int ToId { get; set; }
    }

    /// <summary>
    /// Делегат для обработки события "message_typing_state"
    /// </summary>
    public delegate void MessageTypingStateHandler(object sender, MessageTypingStateArgs messageTypingStateArgs);
}