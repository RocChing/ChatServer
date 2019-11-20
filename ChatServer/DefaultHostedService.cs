using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ChatModel;
using BeetleX;

namespace ChatServer
{
    public class DefaultHostedService : IHostedService
    {
        private readonly ServerConfig serverConfig;
        private readonly ServerOptions serverOptions;
        public DefaultHostedService(IOptions<ServerConfig> options, ServerOptions serverOptions)
        {
            this.serverConfig = options.Value;
            this.serverOptions = serverOptions;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("服务启动");
            //return Task.Factory.StartNew(StartChatServer);
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
            serverOptions.LogLevel = BeetleX.EventArgs.LogType.Info;

            IServer server = SocketFactory.CreateTcpServer<ChatTcpServer>(serverOptions);
            server.Open();

            Console.WriteLine(server);
        }
    }
}
