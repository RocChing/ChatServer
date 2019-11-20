using BeetleX;
using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ChatModel
{
    public class TypeHandler : BeetleX.Packets.IMessageTypeHeader
    {
        private System.Collections.Concurrent.ConcurrentDictionary<Type, string> mTypeNames = new System.Collections.Concurrent.ConcurrentDictionary<Type, string>();

        private System.Collections.Concurrent.ConcurrentDictionary<string, Type> mNameTypes = new System.Collections.Concurrent.ConcurrentDictionary<string, Type>();

        private Type GetType(string typeName)
        {
            Type result;
            if (!mNameTypes.TryGetValue(typeName, out result))
            {
                if (typeName == null)
                    throw new BXException("{0} type not found!", typeName);
                result = Type.GetType(typeName);
                if (result == null)
                    throw new BXException("{0} type not found!", typeName);

                mNameTypes[typeName] = result;
            }
            return result;
        }

        public Type ReadType(PipeStream reader)
        {
            string typeName = reader.ReadShortUTF();
            return GetType(typeName);
        }

        private string GetTypeName(Type type)
        {
            string result;
            if (!mTypeNames.TryGetValue(type, out result))
            {
                TypeInfo info = type.GetTypeInfo();
                if (info.FullName.IndexOf("System") >= 0)
                    result = info.FullName;
                else
                    result = string.Format("{0},{1}", info.FullName, info.Assembly.GetName().Name);
                mTypeNames[type] = result;
            }
            return result;
        }

        public void WriteType(object data, PipeStream writer)
        {
            string name = GetTypeName(data.GetType());
            writer.WriteShortUTF(name);
        }

        public void Register(params Assembly[] assemblies)
        {

        }
    }
}
