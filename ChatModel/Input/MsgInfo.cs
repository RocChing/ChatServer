using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Reflection;

namespace ChatModel.Input
{
    public class MsgInfo
    {
        public CmdType Type { get; set; }

        public object Data { get; set; }

        public MsgInfo() { }

        public MsgInfo(CmdType type, object data)
        {
            Type = type;
            Data = data;
        }

        public T As<T>()
        {
            if (Data is JsonElement ele)
            {
                Console.WriteLine(ele.GetRawText());
                string rowText = ele.GetRawText();
                return JsonSerializer.Deserialize<T>(rowText);
                //var properties = type.GetProperties();
                //T data = Activator.CreateInstance<T>();
                //foreach (PropertyInfo p in properties)
                //{
                //    object value = null;
                //    switch (p.PropertyType.Name)
                //    {
                //        case "Int16":
                //            value = ele.GetInt16();
                //            break;
                //        case "Int32":
                //            value = ele.GetInt32();
                //            break;
                //        case "Int64":
                //            value = ele.GetInt64();
                //            break;
                //        case "string":
                //            value = ele.GetString();
                //            break;
                //    }
                //    p.SetValue(data, value);
                //}
                //return data;
            }
            return default;
        }
    }
}
