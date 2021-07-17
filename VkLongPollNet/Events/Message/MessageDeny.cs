using Newtonsoft.Json;

namespace VkLongPollNet
{
    // TODO Возможен баг (возвращается пустой key)

    /// <summary>
    /// Класс для десериализации LongPoll-события "message_deny" 
    /// </summary>
    public class MessageDenyArgs
    {
        [JsonProperty("user_id")] public int UserId { get; set; }

        [JsonProperty("key")] public string Key { get; set; }
    }

    /// <summary>
    /// Делегат для обработки события "message_deny"
    /// </summary>
    public delegate void MessageDenyHandler(object sender, MessageDenyArgs messageDenyArgs);
}