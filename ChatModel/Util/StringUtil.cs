using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ChatModel.Util
{
    public class StringUtil
    {
        public static string GetMd5String(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var strResult = BitConverter.ToString(result);
                strResult = strResult.Replace("-", "");
                return strResult;
            }
        }

        public static string GetGBString(string content)
        {
            string strreg = @"\\u([0-9a-fA-F]{4})";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(strreg, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //System.Text.RegularExpressions.MatchEvaluator evaluator = new System.Text.RegularExpressions.MatchEvaluator(ReplaceMatchEvaluator);
            string result = reg.Replace(content, ReplaceMatchEvaluator);
            return result;
        }
        private static string ReplaceMatchEvaluator(System.Text.RegularExpressions.Match m)
        {
            string reult = ToGB2312(m.Value);
            return reult;
        }

        /// <summary>
        /// 16进制字符串转为中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ToGB2312(string str)
        {
            string r = "";
            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            byte[] bts = new byte[2];
            foreach (System.Text.RegularExpressions.Match m in mc)
            {
                bts[0] = (byte)int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                bts[1] = (byte)int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                r += Encoding.Unicode.GetString(bts);
            }
            return r;
        }
    }
}
