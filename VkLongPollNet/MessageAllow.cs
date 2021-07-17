using Newtonsoft.Json;

namespace VkLongPollNet
{
    /// <summary>
    /// Класс для десериализации LongPoll-события "message_allow" 
    /// </summary>
    public class MessageAllowArgs
    {
        [JsonProperty("user_id")] public int UserId { get; set; }
    }

    /// <summary>
    /// Делегат для обработки события "message_allow"
    /// </summary>
    public delegate void MessageAllowHandler(object sender, MessageAllowArgs messageAllowArgs);
}