using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Pure.Utils
{
#if !NET45
    public class WebRequestHelper
    {
        public static bool QueryContainsKey(string key, Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.Request.Query.ContainsKey(key);
        }
        public static string QueryRequestValue(string key, Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.Request.Query[key].ToString();
        }

        public static T QueryRequestValue<T>(string key, T defValue, Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.Request.Query[key] == StringValues.Empty ? defValue : ConvertTo<T>(context.Request.Query[key].ToString());
        }
        public static bool FormContainsKey(string key, Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (context.Request.HasFormParams())
            {
                return context.Request.Form.ContainsKey(key);

            }
            return false;
        }
        public static string FormRequestValue(string key, Microsoft.AspNetCore.Http.HttpContext context)
        {
            //if (context.Request.Method == HttpMethods.Post)
            if (context.Request.HasFormParams())
            {
                return context.Request.Form[key] == StringValues.Empty ? "" : context.Request.Form[key].ToString();

            }
            return "";
        }

        public static T FormRequestValue<T>(string key, T defValue, Microsoft.AspNetCore.Http.HttpContext context)
        {
            //if (context.Request.Method ==HttpMethods.Post)
            if (context.Request.HasFormParams())
            {
                return context.Request.Form[key] == StringValues.Empty ? defValue : ConvertTo<T>(context.Request.Form[key].ToString());

            }
            return defValue;

        }
        public static bool RequestContainsKey(string key, Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put || context.Request.Method == HttpMethods.Patch || context.Request.Method == HttpMethods.Delete)
            {
                return FormContainsKey(key, context);
            }

            return QueryContainsKey(key, context);
            
        }
        public static string RequestValue(string key, Microsoft.AspNetCore.Http.HttpContext context)
        {
            string valueQ = QueryRequestValue(key, context);
            string valueF = FormRequestValue(key, context);
            if (!string.IsNullOrEmpty(valueQ))
            {
                return valueQ;
            }
            else if (!string.IsNullOrEmpty(valueF))
            {
                return valueF;
            }
            else
            {
                return "";
            }

        }

        public static T RequestValue<T>(string key, T defValue, Microsoft.AspNetCore.Http.HttpContext context)
        {
            string value = RequestValue(key, context);
            return string.IsNullOrEmpty(value) ? defValue : ConvertTo<T>(value);
        }

        public static T CookieValue<T>(string key, T defValue, Microsoft.AspNetCore.Http.HttpContext context)
        {
            var cookies = context.Request.Cookies;
            string value = "";
            if (cookies.ContainsKey(key))
            {
                value = context.Request.Cookies[key];
            }

            return string.IsNullOrEmpty(value) ? defValue : ConvertTo<T>(value);
        }
        public static string CookieValue(string key, Microsoft.AspNetCore.Http.HttpContext context)
        {
            var cookies = context.Request.Cookies;
            string value = "";
            if (cookies.ContainsKey(key))
            {
                value = context.Request.Cookies[key];
            }

            return value;
        }


        #region 将数据转换为指定类型
        /// <summary>
        /// 将数据转换为指定类型
        /// </summary>
        /// <param name="data">转换的数据</param>
        /// <param name="targetType">转换的目标类型</param>
        public static object ConvertTo(object data, Type targetType)
        {
            //如果数据为空，则返回
            if (IsNullOrEmpty(data))
            {
                return null;
            }

            try
            {
                //如果数据实现了IConvertible接口，则转换类型
                if (data is IConvertible)
                {
                    return Convert.ChangeType(data, targetType);
                }
                else
                {
                    return data;
                }
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// 是否是可空类型（Nullable）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableType(Type type)
        {

            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>
        /// 将数据转换为指定类型
        /// </summary>
        /// <typeparam name="T">转换的目标类型</typeparam>
        /// <param name="data">转换的数据</param>
        public static T ConvertTo<T>(object data)
        {
            //如果数据为空，则返回
            if (IsNullOrEmpty(data))
            {
                return default(T);
            }

            try
            {
                //如果数据是T类型，则直接转换
                if (data is T)
                {
                    return (T)data;
                }
                Type conversionType = typeof(T);
                if (IsNullableType(conversionType))
                {

                    //如果convertsionType为nullable类，声明一个NullableConverter类，该类提供从Nullable类到基础基元类型的转换
                    NullableConverter nullableConverter = new NullableConverter(conversionType);
                    //将convertsionType转换为nullable对的基础基元类型
                    conversionType = nullableConverter.UnderlyingType;

                    return (T)(data == null ? Activator.CreateInstance(conversionType) : Convert.ChangeType(data, conversionType));

                }
                //如果数据实现了IConvertible接口，则转换类型
                if (data is IConvertible)
                {
                    return (T)Convert.ChangeType(data, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }
        #endregion

        #region 判断对象是否为空
        /// <summary>
        /// 判断对象是否为空，为空返回true
        /// </summary>
        /// <typeparam name="T">要验证的对象的类型</typeparam>
        /// <param name="data">要验证的对象</param>        
        public static bool IsNullOrEmpty<T>(T dataSrc)
        {
            //如果为null
            if (dataSrc == null)
            {
                return true;
            }
            //如果为""
            if (dataSrc.GetType() == typeof(String))
            {
                if (string.IsNullOrEmpty(dataSrc.ToString().Trim()))
                {
                    return true;
                }
            }
            //如果为DBNull
            if (dataSrc.GetType() == typeof(DBNull))
            {
                return true;
            }
            //不为空
            return false;
        }

        /// <summary>
        /// 判断对象是否为空，为空返回true
        /// </summary>
        /// <param name="data">要验证的对象</param>
        public static bool IsNullOrEmpty(object objSrc)
        {
            //如果为null
            if (objSrc == null)
            {
                return true;
            }
            //如果为""
            if (objSrc.GetType() == typeof(String))
            {
                if (string.IsNullOrEmpty(objSrc.ToString().Trim()))
                {
                    return true;
                }
            }
            //如果为DBNull
            if (objSrc.GetType() == typeof(DBNull))
            {
                return true;
            }
            //不为空
            return false;
        }
        #endregion



        /// <summary>
        /// 获取请求客户端的Ip地址,无视代理服务器
        /// </summary>
        /// <returns></returns>
        public static string GetIp(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string Ip = "未获取用户IP";
            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return "";
                string CustomerIP = "";
                CustomerIP = context.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP))
                {
                    return CustomerIP;
                }
                CustomerIP = context.Request.Headers["HTTP_X_FORWARDED_FOR"];
                if (!String.IsNullOrEmpty(CustomerIP))
                    return CustomerIP;
                if (context.Request.Headers["HTTP_VIA"] != StringValues.Empty)
                {
                    CustomerIP = context.Request.Headers["HTTP_X_FORWARDED_FOR"];
                    if (CustomerIP == null)
                        CustomerIP = context.Request.Headers["REMOTE_ADDR"];
                }
                else
                {
                    CustomerIP = context.Request.Headers["REMOTE_ADDR"];
                }
                if (string.Compare(CustomerIP, "unknown", true) == 0)
                    return "127.0.0.1";
                return CustomerIP;
            }
            catch
            {
                Ip = "127.0.0.1";
            }
            return Ip;
        }


        /// <summary>
        /// 获取客户端IP地址（无视代理）
        /// </summary>
        /// <returns>若失败则返回回送地址</returns>
        public static string GetClientIP(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string userHostAddress = context.Request.Headers["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(userHostAddress))
            {
                if (context.Request.Headers["HTTP_VIA"] != StringValues.Empty)
                    userHostAddress = context.Request.Headers["HTTP_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim();
            }
            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = "127.0.0.1";
            }

            //最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
            if (!string.IsNullOrEmpty(userHostAddress))
            {
                return userHostAddress;
            }
            return "";
        }

        static Dictionary<string, object> NvcToDictionary(NameValueCollection nvc, bool handleMultipleValuesPerKey)
        {
            var result = new Dictionary<string, object>();
            foreach (string key in nvc.Keys)
            {
                if (handleMultipleValuesPerKey)
                {
                    string[] values = nvc.GetValues(key);
                    if (values.Length == 1)
                    {
                        result.Add(key, values[0]);
                    }
                    else
                    {
                        result.Add(key, values);
                    }
                }
                else
                {
                    result.Add(key, nvc[key]);
                }
            }

            return result;
        }
        public static Dictionary<string, string> GetRequestParams(Microsoft.AspNetCore.Http.HttpContext context)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (context == null || context.Request == null || context.Request.Headers == null)
                return result;

            var request = context.Request;
            Func<Func<HttpRequest, IEnumerable<KeyValuePair<string, StringValues>>>, NameValueCollection> tryGetCollection = getter =>
            {
                try
                {
                    var ps = getter(request);
                    NameValueCollection collections = new NameValueCollection();
                    foreach (var item in ps)
                    {
                        collections.Add(item.Key, item.Value);
                    }
                    return collections;
                }
                catch (Exception e)
                {

                    return new NameValueCollection { { "CollectionFetchError", e.Message } };
                }
            };
            var _queryString = tryGetCollection(r => r.Query);
            var _formString = tryGetCollection(r => r.Form);
            var _query = NvcToDictionary(_queryString, true);
            var _form = NvcToDictionary(_formString, true);
            foreach (var item in _query)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value.ToString());
                }
            }
            foreach (var item in _form)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value.ToString());
                }
            }

            return result;
        }


        /////<summary>
        ///// 获取客户端计算机名称
        /////</summary>
        /////<returns></returns>
        //public static string GetClientComputerName(Microsoft.AspNetCore.Http.HttpContext context)
        //{

        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.Headers == null)
        //            return "";
        //        string clientIP = GetClientIP(context);//获取客户端的IP主机地址
        //        IPHostEntry hostEntry = Dns.GetHostEntry(clientIP);//获取IPHostEntry实体
        //        return hostEntry.HostName;//返回客户端计算机名称
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}

        public static string GetVerb(Microsoft.AspNetCore.Http.HttpContext context)
        {

            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return "";
                return context.Request.Method;
            }
            catch (Exception)
            {
                return "";
            }

        }


        //获取浏览器+版本号
        //public static string GetBrowser(Microsoft.AspNetCore.Http.HttpContext context)
        //{
        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.ServerVariables == null)
        //            return "";
        //        string browsers;
        //        HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
        //        string aa = bc.Browser.ToString();
        //        string bb = bc.Version.ToString();
        //        browsers = aa + bb;
        //        return browsers;
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}
        //public static string GetPlatform(Microsoft.AspNetCore.Http.HttpContext context)
        //{
        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.ServerVariables == null)
        //            return "";
        //        return context.Request.Browser.Platform;

        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}

        //获取操作系统版本号  
        public static string GetOSPlatform(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null)
                    return "";
                string Agent = GetUserAgent(context);// ["HTTP_USER_AGENT"];
                if (Agent.IndexOf("NT 4.0") > 0)
                    return "Windows NT ";
                else if (Agent.Contains("NT 10.0"))
                {
                    return "Windows 10";
                }
                else if (Agent.Contains("NT 6.3"))
                {
                    return "Windows 8.1";
                }
                else if (Agent.Contains("NT 6.2"))
                {
                    return "Windows 8";
                }

                else if (Agent.Contains("NT 6.1"))
                {
                    return "Windows 7";
                }
                else if (Agent.IndexOf("NT 5.0") > 0)
                    return "Windows 2000";
                else if (Agent.IndexOf("NT 5.1") > 0)
                    return "Windows XP";
                else if (Agent.IndexOf("NT 5.2") > 0)
                    return "Windows 2003";
                else if (Agent.IndexOf("NT 6.0") > 0)
                    return "Windows Vista";
                else if (Agent.IndexOf("NT 7.0") > 0)
                    return "Windows 7";
                else if (Agent.IndexOf("NT 8.0") > 0)
                    return "Windows 8";
                else if (Agent.IndexOf("WindowsCE") > 0)
                    return "Windows CE";
                else if (Agent.IndexOf("NT") > 0)
                    return "Windows NT ";
                else if (Agent.IndexOf("9x") > 0)
                    return "Windows ME";
                else if (Agent.IndexOf("98") > 0)
                    return "Windows 98";
                else if (Agent.IndexOf("95") > 0)
                    return "Windows 95";
                else if (Agent.IndexOf("Win32") > 0)
                    return "Win32";
                else if (Agent.IndexOf("Linux") > 0)
                    return "Linux";
                else if (Agent.IndexOf("SunOS") > 0)
                    return "SunOS";
                else if (Agent.IndexOf("Mac") > 0)
                    return "Mac";
                else if (Agent.IndexOf("Linux") > 0)
                    return "Linux";
                else if (Agent.IndexOf("Windows") > 0)
                    return "Windows";
                return "未知类型";
            }
            catch (Exception)
            {
                return "";
            }

        }

        /// <summary>
        /// 根据 UserAgent 获取浏览器名称
        /// </summary>
        public static string GetBrowser(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null)
                    return "";
                string userAgent = GetUserAgent(context);// ["HTTP_USER_AGENT"];
                if (userAgent.Contains("Maxthon"))
                {
                    return "遨游浏览器";
                }
                if (userAgent.Contains("MetaSr"))
                {
                    return "搜狗高速浏览器";
                }
                if (userAgent.Contains("BIDUBrowser"))
                {
                    return "百度浏览器";
                }
                if (userAgent.Contains("QQBrowser"))
                {
                    return "QQ浏览器";
                }
                if (userAgent.Contains("GreenBrowser"))
                {
                    return "Green浏览器";
                }
                if (userAgent.Contains("360se"))
                {
                    return "360安全浏览器";
                }
                if (userAgent.Contains("MSIE 6.0"))
                {
                    return "Internet Explorer 6.0";
                }
                if (userAgent.Contains("MSIE 7.0"))
                {
                    return "Internet Explorer 7.0";
                }
                if (userAgent.Contains("MSIE 8.0"))
                {
                    return "Internet Explorer 8.0";
                }
                if (userAgent.Contains("MSIE 9.0"))
                {
                    return "Internet Explorer 9.0";
                }
                if (userAgent.Contains("MSIE 10.0"))
                {
                    return "Internet Explorer 10.0";
                }
                if (userAgent.Contains("Firefox"))
                {
                    return "Firefox";
                }
                if (userAgent.Contains("Opera"))
                {
                    return "Opera";
                }
                if (userAgent.Contains("Chrome"))
                {
                    return "Chrome";
                }
                if (userAgent.Contains("Safari"))
                {
                    return "Safari";
                }
                return "Other Browser";
            }
            catch (Exception)
            {
                return "";
            }
        
        }


        //public static string GetCPUType(Microsoft.AspNetCore.Http.HttpContext context)
        //{
        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.ServerVariables == null)
        //            return "";
        //        var Request = HttpContext.Current.Request;
        //        string userAgent = Request.UserAgent == null ? "" : Request.UserAgent;
        //        if (Request.ServerVariables["HTTP_UA_CPU"] == null)
        //            return "未知";
        //        else
        //            return Request.ServerVariables["HTTP_UA_CPU"];
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}

        //public static string GetNETCLR(Microsoft.AspNetCore.Http.HttpContext context)
        //{
        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.ServerVariables == null)
        //            return "";
        //        var Request = HttpContext.Current.Request;
        //        if (Request.Browser.ClrVersion == null)
        //            return "不支持";
        //        else
        //            return Request.Browser.ClrVersion.ToString();
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}

        //public static string GetSupportActiveX(Microsoft.AspNetCore.Http.HttpContext context)
        //{
        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.Headers == null)
        //            return "";
        //        var Request = context.Request;

        //        return Request.Browser.ActiveXControls ? "支持" : "不支持";
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}

        //public static string GetSupportCookies(Microsoft.AspNetCore.Http.HttpContext context)
        //{
        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.Headers == null)
        //            return "";
        //        var Request = context.Request;

        //        return Request.Browser.Cookies ? "支持" : "不支持";
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}

        //public static string GetSupportCSS(Microsoft.AspNetCore.Http.HttpContext context)
        //{
        //    try
        //    {
        //        if (context == null || context.Request == null || context.Request.Headers == null)
        //            return "";
        //        var Request = context.Request;

        //        return Request.Browser.SupportsCss ? "支持" : "不支持";
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }

        //}
        public static string GetUserLanguage(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return "";
                var Request = context.Request;

                return context.Request.Headers[HeaderNames.AcceptLanguage].ToString();

            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetWapOrPC(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return "";
                var Request = context.Request;
                string httpAccept = Request.Headers["HTTP_ACCEPT"];
                if (httpAccept == null)
                    return "未知";
                else if (httpAccept.IndexOf("wap") > -1)
                    return "手机";
                else
                    return "计算机";
            }
            catch (Exception)
            {
                return "";
            }


        }

        public static string GetACCEPTENCODING(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return "";
                var Request = context.Request;
                if (Request.Headers["HTTP_ACCEPT_ENCODING"] == StringValues.Empty)
                    return "无";
                else
                    return Request.Headers["HTTP_ACCEPT_ENCODING"];


            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetURL(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null)
                    return "";
                var request = context.Request;
                return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();


            }
            catch (Exception)
            {
                return "";
            }

        }

        public static bool IsAjaxRequest(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return false;
                var request = context.Request.IsAjaxRequest();
                return request;
               // return RequestValue("X-Requested-With", context) == "XMLHttpRequest" || (request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest");
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static string GetUserAgent(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return "";

                return context.Request.Headers[HeaderNames.UserAgent].ToString();
            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string HeaderValue(string header, Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                if (context == null || context.Request == null || context.Request.Headers == null)
                    return "";
                var Request = context.Request;

                return context.Request.Headers[header].ToString();

            }
            catch (Exception)
            {
                return "";
            }

        }

    }
#else
     public class WebRequestHelper
    {
        public static string QueryRequestValue(string key, HttpContext context)
        {
            return context.Request.QueryString[key] == null ? "" : context.Request.QueryString[key].ToString();
        }

        public static T QueryRequestValue<T>(string key, T defValue, HttpContext context)
        {
            return context.Request.QueryString[key] == null ? defValue : ConvertTo<T>(context.Request.QueryString[key].ToString());
        }

        public static string FormRequestValue(string key, HttpContext context)
        {
            return context.Request.Form[key] == null ? "" : context.Request.Form[key].ToString();
        }

        public static T FormRequestValue<T>(string key, T defValue, HttpContext context)
        {
            return context.Request.Form[key] == null ? defValue : ConvertTo<T>(context.Request.Form[key].ToString());
        }

        public static string RequestValue(string key, HttpContext context)
        {
            string valueQ = QueryRequestValue(key, context);
            string valueF = FormRequestValue(key, context);
            if (!string.IsNullOrEmpty(valueQ))
            {
                return valueQ;
            }
            else if (!string.IsNullOrEmpty(valueF))
            {
                return valueF;
            }
            else
            {
                return "";
            }

        }

        public static T RequestValue<T>(string key, T defValue, HttpContext context)
        {
            string value = RequestValue(key, context);
            return string.IsNullOrEmpty(value) ? defValue : ConvertTo<T>(value);
        }

        public static T RequestValue<T>(string key, T defValue)
        {
            return RequestValue<T>(key, defValue, System.Web.HttpContext.Current);
        }

        public static string RequestValue(string key)
        {
            return RequestValue(key, System.Web.HttpContext.Current);
        }
    #region 将数据转换为指定类型
        /// <summary>
        /// 将数据转换为指定类型
        /// </summary>
        /// <param name="data">转换的数据</param>
        /// <param name="targetType">转换的目标类型</param>
        public static object ConvertTo(object data, Type targetType)
        {
            //如果数据为空，则返回
            if (IsNullOrEmpty(data))
            {
                return null;
            }

            try
            {
                //如果数据实现了IConvertible接口，则转换类型
                if (data is IConvertible)
                {
                    return Convert.ChangeType(data, targetType);
                }
                else
                {
                    return data;
                }
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// 是否是可空类型（Nullable）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableType(Type type)
        {

            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>
        /// 将数据转换为指定类型
        /// </summary>
        /// <typeparam name="T">转换的目标类型</typeparam>
        /// <param name="data">转换的数据</param>
        public static T ConvertTo<T>(object data)
        {
            //如果数据为空，则返回
            if (IsNullOrEmpty(data))
            {
                return default(T);
            }

            try
            {
                //如果数据是T类型，则直接转换
                if (data is T)
                {
                    return (T)data;
                }
                Type conversionType = typeof(T);
                if (IsNullableType(conversionType))
                {

                    //如果convertsionType为nullable类，声明一个NullableConverter类，该类提供从Nullable类到基础基元类型的转换
                    NullableConverter nullableConverter = new NullableConverter(conversionType);
                    //将convertsionType转换为nullable对的基础基元类型
                    conversionType = nullableConverter.UnderlyingType;

                    return (T)(data == null ? Activator.CreateInstance(conversionType) : Convert.ChangeType(data, conversionType));

                }
                //如果数据实现了IConvertible接口，则转换类型
                if (data is IConvertible)
                {
                    return (T)Convert.ChangeType(data, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }
    #endregion

    #region 判断对象是否为空
        /// <summary>
        /// 判断对象是否为空，为空返回true
        /// </summary>
        /// <typeparam name="T">要验证的对象的类型</typeparam>
        /// <param name="data">要验证的对象</param>        
        public static bool IsNullOrEmpty<T>(T dataSrc)
        {
            //如果为null
            if (dataSrc == null)
            {
                return true;
            }
            //如果为""
            if (dataSrc.GetType() == typeof(String))
            {
                if (string.IsNullOrEmpty(dataSrc.ToString().Trim()))
                {
                    return true;
                }
            }
            //如果为DBNull
            if (dataSrc.GetType() == typeof(DBNull))
            {
                return true;
            }
            //不为空
            return false;
        }

        /// <summary>
        /// 判断对象是否为空，为空返回true
        /// </summary>
        /// <param name="data">要验证的对象</param>
        public static bool IsNullOrEmpty(object objSrc)
        {
            //如果为null
            if (objSrc == null)
            {
                return true;
            }
            //如果为""
            if (objSrc.GetType() == typeof(String))
            {
                if (string.IsNullOrEmpty(objSrc.ToString().Trim()))
                {
                    return true;
                }
            }
            //如果为DBNull
            if (objSrc.GetType() == typeof(DBNull))
            {
                return true;
            }
            //不为空
            return false;
        }
    #endregion



        /// <summary>
        /// 获取请求客户端的Ip地址,无视代理服务器
        /// </summary>
        /// <returns></returns>
        public static string GetIp()
        {
            string Ip = "未获取用户IP";
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                string CustomerIP = "";
                CustomerIP = System.Web.HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP))
                {
                    return CustomerIP;
                }
                CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!String.IsNullOrEmpty(CustomerIP))
                    return CustomerIP;
                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (CustomerIP == null)
                        CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                if (string.Compare(CustomerIP, "unknown", true) == 0)
                    return System.Web.HttpContext.Current.Request.UserHostAddress;
                return CustomerIP;
            }
            catch
            {
                Ip = "";
            }
            return Ip;
        }


        /// <summary>
        /// 获取客户端IP地址（无视代理）
        /// </summary>
        /// <returns>若失败则返回回送地址</returns>
        public static string GetClientIP()
        {
            string userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(userHostAddress))
            {
                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                    userHostAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim();
            }
            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = HttpContext.Current.Request.UserHostAddress;
            }

            //最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
            if (!string.IsNullOrEmpty(userHostAddress))
            {
                return userHostAddress;
            }
            return "";
        }

        static Dictionary<string, object> NvcToDictionary(NameValueCollection nvc, bool handleMultipleValuesPerKey)
        {
            var result = new Dictionary<string, object>();
            foreach (string key in nvc.Keys)
            {
                if (handleMultipleValuesPerKey)
                {
                    string[] values = nvc.GetValues(key);
                    if (values.Length == 1)
                    {
                        result.Add(key, values[0]);
                    }
                    else
                    {
                        result.Add(key, values);
                    }
                }
                else
                {
                    result.Add(key, nvc[key]);
                }
            }

            return result;
        }
        public static Dictionary<string, string> GetRequestParams( )
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var context = System.Web.HttpContext.Current;
            if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                return result;
          
            var request = context.Request;
            Func<Func<HttpRequest, NameValueCollection>, NameValueCollection> tryGetCollection = getter =>
            {
                try
                {
                    return new NameValueCollection(getter(request));
                }
                catch (HttpRequestValidationException e)
                {

                    return new NameValueCollection { { "CollectionFetchError", e.Message } };
                }
            };
            var _queryString = tryGetCollection(r => r.QueryString);
            var _formString = tryGetCollection(r => r.Form);
            var _query = NvcToDictionary(_queryString, true);
            var _form = NvcToDictionary(_formString, true);
            foreach (var item in _query)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value.ToString());
                }
            }
            foreach (var item in _form)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value.ToString());
                }
            }

            return result;
        }


        ///<summary>
        /// 获取客户端计算机名称
        ///</summary>
        ///<returns></returns>
        public static string GetClientComputerName()
        {

            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                string clientIP = HttpContext.Current.Request.UserHostAddress;//获取客户端的IP主机地址
                IPHostEntry hostEntry = Dns.GetHostEntry(clientIP);//获取IPHostEntry实体
                return hostEntry.HostName;//返回客户端计算机名称
            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetVerb()
        {

            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                return HttpContext.Current.Request.HttpMethod; 
            }
            catch (Exception)
            {
                return "";
            }

        }


        //获取浏览器+版本号
        public static string GetBrowser()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                string browsers;
                HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
                string aa = bc.Browser.ToString();
                string bb = bc.Version.ToString();
                browsers = aa + bb;
                return browsers;
            }
            catch (Exception)
            {
                return "";
            }

        }
        public static string GetPlatform()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                return System.Web.HttpContext.Current.Request.Browser.Platform;

            }
            catch (Exception)
            {
                return "";
            }

        }

        //获取操作系统版本号  
        public static string GetOS()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                string Agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
                if (Agent.IndexOf("NT 4.0") > 0)
                    return "Windows NT ";
                else if (Agent.Contains("NT 10.0"))
                {
                    return "Windows 10";
                }
                else if (Agent.Contains("NT 6.3"))
                {
                    return "Windows 8.1";
                }
                else if (Agent.Contains("NT 6.2"))
                {
                    return "Windows 8";
                }

                else if (Agent.Contains("NT 6.1"))
                {
                    return "Windows 7";
                }
                else if (Agent.IndexOf("NT 5.0") > 0)
                    return "Windows 2000";
                else if (Agent.IndexOf("NT 5.1") > 0)
                    return "Windows XP";
                else if (Agent.IndexOf("NT 5.2") > 0)
                    return "Windows 2003";
                else if (Agent.IndexOf("NT 6.0") > 0)
                    return "Windows Vista";
                else if (Agent.IndexOf("NT 7.0") > 0)
                    return "Windows 7";
                else if (Agent.IndexOf("NT 8.0") > 0)
                    return "Windows 8";
                else if (Agent.IndexOf("WindowsCE") > 0)
                    return "Windows CE";
                else if (Agent.IndexOf("NT") > 0)
                    return "Windows NT ";
                else if (Agent.IndexOf("9x") > 0)
                    return "Windows ME";
                else if (Agent.IndexOf("98") > 0)
                    return "Windows 98";
                else if (Agent.IndexOf("95") > 0)
                    return "Windows 95";
                else if (Agent.IndexOf("Win32") > 0)
                    return "Win32";
                else if (Agent.IndexOf("Linux") > 0)
                    return "Linux";
                else if (Agent.IndexOf("SunOS") > 0)
                    return "SunOS";
                else if (Agent.IndexOf("Mac") > 0)
                    return "Mac";
                else if (Agent.IndexOf("Linux") > 0)
                    return "Linux";
                else if (Agent.IndexOf("Windows") > 0)
                    return "Windows";
                return "未知类型";
            }
            catch (Exception)
            {
                return "";
            }

        }


        public static string GetCPUType()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;
                string userAgent = Request.UserAgent == null ? "" : Request.UserAgent;
                if (Request.ServerVariables["HTTP_UA_CPU"] == null)
                    return "未知";
                else
                    return Request.ServerVariables["HTTP_UA_CPU"];
            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetNETCLR()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;
                if (Request.Browser.ClrVersion == null)
                    return "不支持";
                else
                    return Request.Browser.ClrVersion.ToString();
            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetSupportActiveX()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;

                return Request.Browser.ActiveXControls ? "支持" : "不支持";
            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetSupportCookies()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;

                return Request.Browser.Cookies ? "支持" : "不支持";
            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetSupportCSS()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;

                return Request.Browser.SupportsCss ? "支持" : "不支持";
            }
            catch (Exception)
            {
                return "";
            }

        }
        public static string GetUserLanguage()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;
                if (Request.UserLanguages != null && Request.UserLanguages.Length > 0)
                {
                    return Request.UserLanguages[0];

                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetWapOrPC()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;
                string httpAccept = Request.ServerVariables["HTTP_ACCEPT"];
                if (httpAccept == null)
                    return "未知";
                else if (httpAccept.IndexOf("wap") > -1)
                    return "手机";
                else
                    return "计算机";
            }
            catch (Exception)
            {
                return "";
            }


        }

        public static string GetACCEPTENCODING()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;
                if (Request.ServerVariables["HTTP_ACCEPT_ENCODING"] == null)
                    return "无";
                else
                    return Request.ServerVariables["HTTP_ACCEPT_ENCODING"];


            }
            catch (Exception)
            {
                return "";
            }

        }

        public static string GetURL()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                string url = System.Web.HttpContext.Current.Request.Url.ToString();
                return url;


            }
            catch (Exception)
            {
                return "";
            }

        }

        public static bool IsAjaxRequest()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return false;
                var request = System.Web.HttpContext.Current.Request;
                return request["X-Requested-With"] == "XMLHttpRequest" || (request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest");
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static string GetUserAgent()
        {
            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";
                var Request = HttpContext.Current.Request;
                string userAgent = Request.UserAgent == null ? "无" : Request.UserAgent;
                return userAgent;
            }
            catch (Exception)
            {
                return "";
            }

        }



    }
#endif
}
