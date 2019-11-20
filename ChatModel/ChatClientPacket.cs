using BeetleX.Buffers;
using BeetleX.Clients;
using BeetleX.Packets;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace ChatModel
{
    public class ChatClientPacket : FixeHeaderClientPacket
    {
        public ChatClientPacket()
        {
            TypeHeader = new TypeHandler();
        }

        public void Register(params Assembly[] assemblies)
        {
            TypeHeader.Register(assemblies);
        }

        public IMessageTypeHeader TypeHeader { get; set; }


        public override IClientPacket Clone()
        {
            ChatClientPacket result = new ChatClientPacket();
            result.TypeHeader = TypeHeader;
            return result;
        }

        protected override object OnRead(IClient client, PipeStream stream)
        {
            Type type = TypeHeader.ReadType(stream);
            string value = stream.ReadUTF();
            return JsonSerializer.Deserialize(value, type);
        }

        protected override void OnWrite(object data, IClient client, PipeStream stream)
        {
            TypeHeader.WriteType(data, stream);
            string value = JsonSerializer.Serialize(data);
            stream.WriteUTF(value);
        }
    }
}
