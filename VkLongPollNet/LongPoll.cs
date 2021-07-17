using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Model;
using VkNet.Utils;

namespace VkLongPollNet
{
    /// <summary>
    /// <para>
    /// Класс для десериализации ответа на запрос LongPoll события.
    /// </para>
    /// <para>
    /// <a href="https://vk.com/dev/bots_longpoll?f=2.%20%D0%A4%D0%BE%D1%80%D0%BC%D0%B0%D1%82%20%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85">Документация vk-api</a>
    /// </para>
    /// </summary>
    // Отключение варнингов, поскольку класс необходим для десериализации json
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    internal class LongPollEventRoot
    {
        /// <summary>
        /// <para>
        /// Номер последнего события, начиная с которого нужно получать данные.
        /// </para>
        /// <para>
        /// Чтобы LongPoll сервер работал непрерывно нужно менять <c>ts</c> при получении нового события.
        /// </para>
        /// </summary>
        [JsonPropertyName("ts")]
        public string Ts { get; set; }

        /// <summary>
        /// <seealso cref="LongPollEventUpdate"/>
        /// </summary>
        [JsonPropertyName("updates")]
        public List<LongPollEventUpdate> Updates { get; set; }
    }

    /// <summary>
    /// Класс для десериализации свойства "updates" из json-ответа, полученного при запросе LongPoll события.
    /// </summary>
    /// <remarks>
    /// <a href="https://vk.com/dev/bots_longpoll?f=2.%20%D0%A4%D0%BE%D1%80%D0%BC%D0%B0%D1%82%20%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85">Документация vk-api</a>
    /// </remarks>
    // Отключение варнингов, поскольку класс необходим для десериализации json
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    internal class LongPollEventUpdate
    {
        /// <summary>
        /// Определяет тип события, возвращенного LongPoll сервером.
        /// </summary>
        /// <remarks>
        /// <a href="https://vk.com/dev/groups_events">Типы событий и прочее </a>
        /// </remarks>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <remarks>
        /// Поле Object имеет тип JObject, поскольку заранее не извествен тип события.
        /// В связи с этим каждый раз нужно десериализовать объект вручную.
        /// </remarks>
        /// <example>
        /// <code>
        /// MessageNewArgs messageNewArgs = update.Object.ToObject<MessageNewArgs/>();
        /// </code>
        /// </example>
        [JsonPropertyName("object")]
        public JObject Object { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; }
    }

    /// <summary>
    /// <para>
    /// Класс для работы с VK LongPoll API
    /// </para>
    /// <seealso cref="StartListening"/>
    /// </summary>
    public class LongPoll
    {
        // Здесь перечислены все события, которые может вызывать LongPoll сервер
        // Не забудьте включить прием нужных вам событий в настройках LongPoll
        // Это можно сделать непостредственно в группе: https://vk.com/{group_name}?act=longpoll_api_types
        // Или с помощью метода Groups.setLongPollSettings
        public event MessageNewHandler MessageNew;
        public event MessageReplyHandler MessageReply;
        public event MessageEditHandler MessageEdit;
        public event MessageAllowHandler MessageAllow;
        public event MessageDenyHandler MessageDeny;
        public event MessageTypingStateHandler MessageTypingState;

        /// <summary>
        /// Основное поле, содержащее поля Ts, Server и Key, небходимые для корректной работы LongPoll.
        /// </summary>
        /// <seealso cref="LongPollServerResponse"/>
        private readonly LongPollServerResponse _longPollServerResponse;

        /// <summary>
        /// Необходим для реализации включения и отключения работы LongPoll сервера
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Логгирование
        /// </summary>
        private readonly ILogger _logger;

        private readonly ServiceProvider _serviceProvider;

        /// <remarks>
        /// Библиотека позиционирует себя как модуль библиотеки <a href="https://github.com/vknet/vk">VkNet</a>
        /// Поэтому реализована она с помощью соответсвующих классов и методов из неё.
        /// Работать независимо от VkNet она не умеет!
        /// </remarks>
        /// <param name="longPollServerResponse">Объект содержащий поля Ts, Server и Key, небходимые для корректной работы LongPoll.</param>
        /// <param name="logger">Логгер</param>
        /// <param name="serviceCollection">TODO</param>
        /// <seealso cref="LongPollServerResponse"/>
        public LongPoll(LongPollServerResponse longPollServerResponse, IServiceCollection serviceCollection = null)
        {
            this._longPollServerResponse = longPollServerResponse;

            var container = serviceCollection ?? new ServiceCollection();

            container.RegisterDefaultDependencies();

            _serviceProvider = container.BuildServiceProvider();

            _logger = _serviceProvider.GetService<ILogger<LongPoll>>();
        }

        /// <summary>
        /// Метод Listen автоматически отправляет запросы на получение событий к LongPoll серверу.
        /// Когда приходит ответ, определяется тип события, а затем вызывается <c>event?.Invoke()</c>, который требуется обработать пользователю.
        /// Для детального описания аргументов см. документацию: <a href="https://vk.com/dev/using_longpoll?f=1.%20%D0%9F%D0%BE%D0%B4%D0%BA%D0%BB%D1%8E%D1%87%D0%B5%D0%BD%D0%B8%D0%B5">Vk API</a>
        /// </summary>
        /// <param name="wait">время ожидания (так как некоторые прокси-серверы обрывают соединение после 30 секунд, мы рекомендуем указывать wait=25). Максимальное значение — 90. </param>
        /// <param name="mode">дополнительные опции ответа.</param>
        /// <param name="version">версия</param>
        public async Task StartListening(int wait = 20, int mode = 2, int version = 2)
        {
            _logger.LogDebug($"Start listening LongPoll....");
            // TODO запретить дважды вызывать метод  StartListening или как-то обезопасить этот вызов

            // Создаем токен, позволяющий пользователю остановить сервер
            _cancellationTokenSource = new CancellationTokenSource();
            // Работаем до тех пор, пока токен активен
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Получаем очередное событие
                LongPollEventRoot lpEvent = await GetEventFromLongPoll(wait, mode, version);
                // Обновляем ts
                _longPollServerResponse.Ts = lpEvent.Ts;
                // Обрабатываем каждое событие из списка
                foreach (var update in lpEvent.Updates)
                {
                    // Далее определяется тип события и вызываетя соответствующее событие
                    // Поскольку в ответе к Api не было понятно, какой тип события был возращен
                    // Необходимо дополнительно десериализовать каждый объект в уже установленный тип

                    switch (update.Type)
                    {
                        case "message_new":
                            MessageNew?.Invoke(this, update.Object.ToObject<MessageNewArgs>());
                            break;
                        case "message_reply":
                            MessageReply?.Invoke(this, update.Object.ToObject<MessageReplyArgs>());
                            break;
                        case "message_edit":
                            MessageEdit?.Invoke(this, update.Object.ToObject<MessageEditArgs>());
                            break;
                        case "message_allow":
                            MessageAllow?.Invoke(this, update.Object.ToObject<MessageAllowArgs>());
                            break;
                        case "message_deny":
                            MessageDeny?.Invoke(this, update.Object.ToObject<MessageDenyArgs>());
                            break;
                        case "message_typing_state":
                            MessageTypingState?.Invoke(this, update.Object.ToObject<MessageTypingStateArgs>());
                            break;
                        case "message_event":
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Останавливает LongPoll сервер
        /// </summary>
        public void StopListening()
        {
            _logger.LogDebug($"Stop listening LongPoll...");
            this._cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Формирует запрос к LongPoll Server и возвращает десериализованный ответ от сервера.
        /// Для детального описания аргументов см. документацию: <a href="https://vk.com/dev/using_longpoll?f=1.%20%D0%9F%D0%BE%D0%B4%D0%BA%D0%BB%D1%8E%D1%87%D0%B5%D0%BD%D0%B8%D0%B5">Vk API</a>
        /// </summary>
        /// <param name="wait">время ожидания (так как некоторые прокси-серверы обрывают соединение после 30 секунд, мы рекомендуем указывать wait=25). Максимальное значение — 90. </param>
        /// <param name="mode">дополнительные опции ответа.</param>
        /// <param name="version">версия</param>
        /// <returns>Десериализованный ответ от сервера типа <c>LongPollEventRoot</c></returns>
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

            _logger?.LogDebug(
                $"LongPoll response:{Environment.NewLine}{Utilities.PrettyPrintJson(await response.Content.ReadAsStringAsync())}");
            return JsonConvert.DeserializeObject<LongPollEventRoot>(await response.Content.ReadAsStringAsync());
        }
    }
}