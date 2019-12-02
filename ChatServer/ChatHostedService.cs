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
            //byte[] bytes = Encoding.UTF8.GetBytes("N");
            //foreach (var item in bytes)
            //{
            //    Console.WriteLine(item);
            //}
            //string info = Encoding.UTF8.GetString(bytes);
            //Console.WriteLine(info);
            //byte[] bytes = BitConverter.GetBytes(64);
            //byte[] bytes = intToBytes(64);
            //foreach (var item in bytes)
            //{
            //    Console.WriteLine(item);
            //}

            //byte[] bytes = new byte[4] { 0, 0, 0, 154 };
            //int res = byteArrayToInt2(bytes);
            //Console.WriteLine($"the res value is:" + res);
            StartChatServer();
            return Task.FromResult(0);
        }

        public byte[] intToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
        }

        public int byteArrayToInt2(byte[] b)
        {
            return b[0] & 0xFF |
                    (b[1] & 0xFF) << 8 |
                    (b[2] & 0xFF) << 16 |
                    (b[3] & 0xFF) << 24;
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
