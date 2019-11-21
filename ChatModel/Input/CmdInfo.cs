using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Reflection;

namespace ChatModel.Input
{
    public class CmdInfo
    {
        private string validString;
        public CmdType Type { get; set; }

        public object Data { get; set; }

        public string Token { get; set; }

        public CmdInfo() { }

        public CmdInfo(string vs, CmdType type, object data)
        {
            Type = type;
            Data = data;
            validString = vs;
            SetToken(validString);
        }

        public T As<T>()
        {
            string text = GetDataRowText();
            if (text.IsNullOrEmpty()) return default;
            return JsonSerializer.Deserialize<T>(text);
        }

        public string GetDataRowText()
        {
            if (Data is JsonElement ele)
            {
                return ele.GetRawText();
            }
            return string.Empty;
        }

        public bool IsValid(string validString)
        {
            string json = GetDataRowText();
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

        public CmdInfo Clone(object data)
        {
            return new CmdInfo(validString, Type, data);
        }
    }
}
