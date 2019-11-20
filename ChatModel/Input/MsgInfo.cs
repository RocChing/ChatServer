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

        public string Token { get; set; }

        public MsgInfo() { }

        public MsgInfo(string validString, CmdType type, object data)
        {
            Type = type;
            Data = data;
            SetToken(validString);
        }

        public T As<T>()
        {
            if (Data is JsonElement ele)
            {
                string rowText = ele.GetRawText();
                return JsonSerializer.Deserialize<T>(rowText);
            }
            return default;
        }

        public bool IsValid(string validString)
        {
            string json = string.Empty;
            if (Data is JsonElement ele)
            {
                json = ele.GetRawText();
            }
            string input = $"{validString}-{json}";
            string md5String = Util.StringUtil.GetMd5String(input);
            return md5String.Equals(Token, StringComparison.OrdinalIgnoreCase);
        }

        private void SetToken(string validString)
        {
            string json = JsonSerializer.Serialize(Data);
            string input = $"{validString}-{json}";
            string md5String = Util.StringUtil.GetMd5String(input);
            Token = md5String;
        }
    }
}
