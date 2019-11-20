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

namespace ChatServer
{
    public class DefaultHostedService : IHostedService
    {
        private readonly ServerConfig serverConfig;
        private readonly ServerOptions serverOptions;
        private readonly IServerHandler  serverHandler;
        public DefaultHostedService(IOptions<ServerConfig> options, ServerOptions serverOptions, IServerHandler  serverHandler)
        {
            this.serverConfig = options.Value;
            this.serverOptions = serverOptions;
            this.serverHandler = serverHandler;
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
            serverOptions.LogLevel = LogType.Info;

            IServer server = SocketFactory.CreateTcpServer(serverHandler, null, serverOptions);
            server.Open();

            Console.WriteLine(server);
        }
    }
}
