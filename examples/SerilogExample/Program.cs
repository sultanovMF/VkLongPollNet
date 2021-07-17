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
            
            var services = new ServiceCollection();
            
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddSerilog(dispose: true);
            });
            
            var vkApi = new VkApi(services);
            
            vkApi.Authorize(new ApiAuthParams { AccessToken = "access_token" });
            var longPollServer = vkApi.Groups.GetLongPollServer(1);
            var longPoll = new LongPoll(longPollServer, services);
            await longPoll.StartListening();
        }
    }
}