using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BeetleX;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using BeetleX.Packets;
using System.Text.Json;

namespace ChatModel
{
    public class ChatPacket : FixedHeaderPacket
    {
        public ChatPacket()
        {
            TypeHeader = new TypeHandler();
        }

        private PacketDecodeCompletedEventArgs mCompletedEventArgs = new PacketDecodeCompletedEventArgs();

        public void Register(params Assembly[] assemblies)
        {
            TypeHeader.Register(assemblies);
        }

        public IMessageTypeHeader TypeHeader { get; set; }

        public override IPacket Clone()
        {
            ChatPacket result = new ChatPacket();
            result.TypeHeader = TypeHeader;
            return result;
        }

        protected override object OnReader(ISession session, PipeStream reader)
        {
            Type type = TypeHeader.ReadType(reader);
            string value = reader.ReadUTF();
            return JsonSerializer.Deserialize(value, type);
        }

        protected override void OnWrite(ISession session, object data, PipeStream writer)
        {
            TypeHeader.WriteType(data, writer);
            string value = JsonSerializer.Serialize(data);
            writer.WriteUTF(value);
        }
    }
}
