using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

using BeetleX;
using ChatRepository;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(Guid.NewGuid().ToString("N"));
            //Console.ReadLine();
            //return;
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
                   services.AddHostedService<ChatHostedService>();

                   services.AddDbContext<ChatDbContext>(opt =>
                   {
                       string connectString = context.Configuration.GetConnectionString("ChatDb");
                       opt.UseSqlite(connectString);
                   });

                   services.AddScoped<IUserRepository, UserRepository>();

                   services.AddSingleton(new ServerOptions());
                   services.AddScoped<IServerHandler, ChatTcpServer>();
               })
               .Build();

            host.Run();
        }
    }
}
