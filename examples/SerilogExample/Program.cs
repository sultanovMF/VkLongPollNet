using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkLongPollNet;
namespace hueta
{
    class Program
    {
        /*
         * Смотрите основую информацию здесь.
         * https://github.com/vknet/vk/wiki/Logging
         */
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel
                .Debug()
                .WriteTo
                .Console()
                .WriteTo
                .File("log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
            // Контейнер для инверсии зависимостей
            var services = new ServiceCollection();

            // Регистрация логгера
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddSerilog(dispose: true);
            });
            
            var vkApi = new VkApi(services);
            
            vkApi.Authorize(new ApiAuthParams { AccessToken = "90367ca154c6e29bde05193361b81af9701ed7e342c71813f53e36d6a9eb6773476eae66a5254eb124c3d" });
            var longPollServer = vkApi.Groups.GetLongPollServer(205136689);
            var longPoll = new LongPoll(longPollServer, services);
            await longPoll.StartListening();
        }
    }
}