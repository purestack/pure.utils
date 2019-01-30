using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Pure.Utils
{
    /// <summary>
    /// 短链生成
    /// </summary>
    public class ShortUrlBuilder
    {
        private static readonly string[] Chars =
        { "a" , "b" , "c" , "d" , "e" , "f" , "g" , "h" ,

            "i" , "j" , "k" , "l" , "m" , "n" , "o" , "p" , "q" , "r" , "s" , "t" ,

            "u" , "v" , "w" , "x" , "y" , "z" , "0" , "1" , "2" , "3" , "4" , "5" ,

            "6" , "7" , "8" , "9" , "A" , "B" , "C" , "D" , "E" , "F" , "G" , "H" ,

            "I" , "J" , "K" , "L" , "M" , "N" , "O" , "P" , "Q" , "R" , "S" , "T" ,

            "U" , "V" , "W" , "X" , "Y" , "Z"
            };

        /// <summary>
        /// 生成指定URL的摘要
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>短链列表，任取一个存储就行。</returns>
        public static List<string> Build(string url)
        {
            var shortUrls = new List<string>();
            if (url == null)
            {
                return shortUrls;
            }
            var hash = GetMd5(url);
            //将32个字符的md5码分成4段处理，每段8个字符
            for (int i = 0; i < 4; i++)
            {

                int offset = i * 8;

                string sub = hash.Substring(offset, 8);

                long sub16 = long.Parse(sub, NumberStyles.HexNumber); //将sub当作一个16进制的数，转成long

                // & 0X3FFFFFFF，去掉最前面的2位，只留下30位
                sub16 &= 0X3FFFFFFF;

                StringBuilder sb = new StringBuilder();
                //将剩下的30位分6段处理，每段5位
                for (int j = 0; j < 6; j++)
                {
                    //得到一个 <= 61的数字
                    long t = sub16 & 0x0000003D;
                    sb.Append(Chars[(int)t]);

                    sub16 >>= 5;  //将sub16右移5位
                }

                shortUrls.Add(sb.ToString());
            }
            return shortUrls;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="myString">原始字符串</param>
        /// <param name="salt">盐</param>
        /// <returns></returns>
        private static string GetMd5(string myString, string salt = "")
        {
            if (myString != null)
            {
                var array = myString.ToCharArray();
                Array.Reverse(array);
                myString = $"{new string(array)}+{{{salt}}}";

#if DEBUG
                Debugger.Log(1, "2", $"自定义消息：{myString}\r\n");
#endif
                MD5 md5 = MD5.Create();
                byte[] bs = Encoding.UTF8.GetBytes(myString);
                byte[] hs = md5.ComputeHash(bs);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hs)
                {
                    // 以十六进制格式格式化  
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();

            }
            return null;
        }
    }
}
