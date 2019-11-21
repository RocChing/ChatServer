using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ChatModel;
using BeetleX;
using BeetleX.EventArgs;
using ChatRepository;
using ChatModel.Entity;

namespace ChatServer
{
    public class ChatHostedService : IHostedService
    {
        private readonly ServerConfig serverConfig;
        private readonly ServerOptions serverOptions;
        private readonly IServerHandler serverHandler;

        private readonly IUserRepository userRepository;
        public ChatHostedService(IOptions<ServerConfig> options, ServerOptions serverOptions, IServerHandler serverHandler, IUserRepository userRepository)
        {
            this.serverConfig = options.Value;
            this.serverOptions = serverOptions;
            this.serverHandler = serverHandler;
            this.userRepository = userRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("服务启动");
            StartChatServer();
            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("服务关闭");
            return Task.FromResult(0);
        }

        private void StartChatServer()
        {
            serverOptions.Port = serverConfig.Port;
            serverOptions.LogLevel = LogType.Info;

            IServer server = SocketFactory.CreateTcpServer(serverHandler, null, serverOptions);
            server.Open();

            Console.WriteLine(server);
        }
    }
}
