using System;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkLongPollNet;
class Program
{
    // Создаем объект VkAPi
    private static readonly VkApi VkApi = new VkApi();
    // Обработчик события входящего сообщения
    private static void MessageNewHandler(object sender, MessageNewArgs messageNewArgs)
    {
        // Если пришло сообщение, то отправляем сообщение "test" в тот же чат
        VkApi.Messages.Send(new MessagesSendParams()
        {
            PeerId = messageNewArgs.Message.PeerId,
            Message = "test",
            RandomId = new Random().Next(Int32.MinValue, Int32.MaxValue)
        });
    }

    static async Task Main(string[] args)
    {
        // Вводим ключ доступа сообщества, авторизируем бота 
        VkApi.Authorize(new ApiAuthParams { AccessToken = "access_token" });
        // Вместо единицы подставьте id вашей группы (в формате unsigned long)
        var longPollServer = VkApi.Groups.GetLongPollServer(1);
        // Создаем LongPoll сервер
        var longPoll = new LongPoll(longPollServer);
        // Добавляем обработчик событий
        longPoll.MessageNew += MessageNewHandler;
        // Запускаем сервер
        await longPoll.StartListening();
    }
}