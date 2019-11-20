using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

using BeetleX;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            StartServer();
        }

        static void StartServer()
        {
            var host = new HostBuilder()
               .ConfigureHostConfiguration(config =>
               {
                   config.SetBasePath(Directory.GetCurrentDirectory());
               })
               .ConfigureAppConfiguration(config =>
               {
                   config.AddJsonFile("appsettings.json", false, true);
               })
               .ConfigureLogging((context, configLogging) =>
               {
                   configLogging.ClearProviders();
                   configLogging.AddConfiguration(context.Configuration);
                   configLogging.SetMinimumLevel(LogLevel.Trace);
                   configLogging.AddConsole();
               })
               .ConfigureServices((context, services) =>
               {
                   services.Configure<ServerConfig>(context.Configuration.GetSection(nameof(ServerConfig)));
                   services.AddHostedService<DefaultHostedService>();

                   services.AddSingleton(new ServerOptions());
                   
               })
               .Build();

            host.Run();
        }
    }
}
