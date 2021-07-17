
# VkLongPollNet

Библиотека является расширением [VkNet](https://github.com/vknet/vk) и реализует LongPoll сервер.

❗ Библиотека зависит от [VkNet](https://github.com/vknet/vk) и не может быть использована отдельно от неё до тех пока автор перестанет лениться и сделает что-то вменяемое.

❗ Библиотека оттестирована для работы с Bots LongPollApi. Как она поведет себя при работе с User Long Poll Api не известно.
## Usage

```c#
class Program
{
    // Создаем объект VkAPi
    static private VkApi vkApi = new VkApi();
    // Обработчик события входящего сообщения
    public static void MessageNewHandler(object sender, MessageNewArgs messageNewArgs)
    {
        // Если пришло сообщение, то отправляем сообщение "test" в тот же чат
        vkApi.Messages.Send(new MessagesSendParams()
        {
            PeerId = messageNewArgs.Message.PeerId,
            Message = "test",
            RandomId = new Random().Next(Int32.MinValue, Int32.MaxValue)
        });
    }

    static async Task Main(string[] args)
    {
        // Вводим ключ доступа сообщества, авторизируем бота 
        vkApi.Authorize(new ApiAuthParams { AccessToken = "access_token" });
        // Необходимо получить id сообщества, от лица которого работает бот
        int groupId = 111111111;
        // Для того, чтобы получить данные, необходимые для запуска LongPoll
        var longPollServer = vkApi.Groups.GetLongPollServer(groupId);
        // Создаем LongPoll сервер
        var longPoll = new LongPoll(longPollServer);
        // Добавляем обработчик событий
        longPoll.MessageNew += MessageNewHandler;
        // Запускаем сервер
        await longPoll.Listen();
    }
}

```

## Contributing
Буду благодарен если кто-нибудь поможет в распространении и улучшении работы библиотеки. 
## License
[MIT](https://choosealicense.com/licenses/mit/)