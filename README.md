
# VkLongPollNet

Библиотека является расширением [VkNet](https://github.com/vknet/vk) и реализует LongPoll сервер.

❗ Библиотека зависит от [VkNet](https://github.com/vknet/vk) и не может быть использована отдельно от неё до тех пор, пока автор не перестанет лениться и сделает что-то вменяемое.

❗ Библиотека оттестирована для работы с Bots LongPollAPI. Как она поведет себя при работе с User LongPollAPI не известно.
## Usage

```c#
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
```

Кроме того, сервер не обязательно вызывать в ассинхронном режиме. Он может быть остановлен.
```c#
...
longPoll.StartListening();
Task.Delay(60*1000);
LongPoll.StopListening();
// Сервер больше не выкидывает события
...
```
## Contributing
Буду благодарен, если кто-нибудь поможет в распространении и улучшении работы библиотеки. 
## License
[MIT](https://choosealicense.com/licenses/mit/)