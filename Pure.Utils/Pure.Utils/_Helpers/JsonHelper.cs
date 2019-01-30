using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pure.Utils
{
    /// <summary>
    /// Json字符串操作辅助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 转换实体为JSON字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj, Formatting type)
        {
            return JsonConvert.SerializeObject(obj, type);//Formatting.Indented//Indented表示以缩进形式显示结果
        }
        /// <summary>
        /// 转换实体为JSON字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            return JsonConvert.SerializeObject(obj, timeFormat);
        }

        /// <summary>
        /// 转换Json字符串到实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strJson"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string strJson)
        {
            T destObj = (T)JsonConvert.DeserializeObject(strJson, typeof(T));
            return destObj;
        }

        public static string ConvertXmlToJson(System.Xml.XmlDocument doc)
        {
            return JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
        }

        public static System.Xml.XmlDocument ConvertJsonToXml(string jsonText)
        {
            return (System.Xml.XmlDocument)JsonConvert.DeserializeXmlNode(jsonText);
        }

    }

}
