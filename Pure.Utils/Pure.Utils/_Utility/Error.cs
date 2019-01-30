using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pure.Utils
{
    public class Error
    {
        private NameValueCollection _serverVariables;
        private NameValueCollection _queryString;
        private NameValueCollection _form;
        private NameValueCollection _cookies;
        private NameValueCollection _requestHeaders;

        internal const string CollectionErrorKey = "CollectionFetchError";

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        public Error() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class from a given <see cref="Exception"/> instance.
        /// </summary>
        public Error(Exception e) : this(e, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class
        /// from a given <see cref="Exception"/> instance and 
        /// <see cref="HttpContext"/> instance representing the HTTP 
        /// context during the exception.
        /// </summary>
        public Error(Exception e, Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (e == null) throw new ArgumentNullException("e");

            Exception = e;
            var baseException = e;

            // if it's not a .Net core exception, usually more information is being added
            // so use the wrapper for the message, type, etc.
            // if it's a .Net core exception type, drill down and get the innermost exception
            if (IsBuiltInException(e))
                baseException = e.GetBaseException();
            MachineName = Environment.MachineName;
            Source = baseException.Source;
            Detail = e.ToString();
            var httpException = e ;
            if (httpException != null)
            {
                StatusCode = 500;// httpException.GetHttpCode();
            }
            else
            {
                StatusCode = context != null ? context.Response.StatusCode : 0;
                //if (e is NotImplementedException)
                //{
                //    StatusCode = HttpStatusCode.NotImplemented.GetHashCode();
                //}
                ////错误码 408 超时
                //else if (e is TimeoutException)
                //{
                //    StatusCode = (HttpStatusCode.RequestTimeout).GetHashCode();
                //}
                ////错误码 403 拒绝访问
                //else if (e is NotImplementedException)
                //{
                //    StatusCode = (HttpStatusCode.Forbidden).GetHashCode();

                //}
                ////错误码404
                //else if (e is NotImplementedException)
                //{
                //    StatusCode = (HttpStatusCode.NotFound).GetHashCode();

                //}
                //else
                //{
                //    StatusCode = (HttpStatusCode.InternalServerError).GetHashCode();
                //}
            }

            SetContextProperties(context);
        }



        /// <summary>
        /// Sets Error properties pulled from HttpContext, if present
        /// </summary>
        /// <param name="context">The HttpContext related to the request</param>
        private void SetContextProperties(Microsoft.AspNetCore.Http.HttpContext  context)
        {
            if (context == null) return;

            var request = context.Request;
            AbsUrl = request.GetUrl().ToString();
            Func<Func<HttpRequest, IQueryCollection>, NameValueCollection> tryGetCollection = getter =>
            {
                try
                {
                    var result = new NameValueCollection();
                    foreach (var item in getter(request))
                    {
                        result.Add(item.Key, item.Value);
                    }

                    return result;

                }
                catch (Exception e)
                {
                    // Trace.WriteLine("Error parsing collection: " + e.Message);
                    return new NameValueCollection { { CollectionErrorKey, e.Message } };
                }
            };
            Func<Func<HttpRequest, IFormCollection>, NameValueCollection> tryGetCollectionForm = getter =>
            {
                try
                {
                    var result = new NameValueCollection();
                    foreach (var item in getter(request))
                    {
                        result.Add(item.Key, item.Value);
                    }

                    return result;

                }
                catch (Exception e)
                {
                    // Trace.WriteLine("Error parsing collection: " + e.Message);
                    return new NameValueCollection { { CollectionErrorKey, e.Message } };
                }
            };
            Func<Func<HttpRequest, IHeaderDictionary>, NameValueCollection> tryGetCollectionServerVars = getter =>
            {
                try
                {
                    var result = new NameValueCollection();
                    foreach (var item in getter(request))
                    {
                        result.Add(item.Key, item.Value);
                    }

                    return result;
                }
                catch (Exception e)
                {

                    return new NameValueCollection { { CollectionErrorKey, e.Message } };
                }
            };
            //_serverVariables = tryGetCollectionServerVars(r => r.GetServerVariables());
            _queryString = tryGetCollection(r => r.Query);
            if (request.HasFormParams())
            {
                _form = tryGetCollectionForm(r => r.Form);

            }
            try
            {
                _cookies = new NameValueCollection(request.Cookies.Count);
                foreach (var item in request.Cookies)
                {
                    _cookies.Add(item.Key, item.Value);
                }

            }
            catch
            {
                //Trace.WriteLine("Error parsing cookie collection: " + e.Message);
            }

            _requestHeaders = new NameValueCollection(request.Headers.Count);
            foreach (var header in request.Headers)
            {
                // Cookies are handled above, no need to repeat
                if (string.Compare(header.Key, "Cookie", StringComparison.OrdinalIgnoreCase) == 0)
                    continue;

                _requestHeaders.Add(header.Key, header.Value);
            }
        }
        /// <summary>
        /// returns if the type of the exception is built into .Net core
        /// </summary>
        /// <param name="e">The exception to check</param>
        /// <returns>True if the exception is a type from within the CLR, false if it's a user/third party type</returns>
        private bool IsBuiltInException(Exception e)
        {
            return e.GetType().Module.ScopeName == "CommonLanguageRuntimeLibrary";
        }


        public Exception Exception { get; set; }

        public string AbsUrl { get; set; }
        /// <summary>
        /// Gets the hostname of where the exception occured
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets the source of this error
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets the detail/stack trace of this error
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// Gets the HTTP Status code associated with the request
        /// </summary>
        public int StatusCode { get; set; }

        ///// <summary>
        ///// The URL host of the request causing this error
        ///// </summary>
        //public string Host { get { return _host ?? (_host = _serverVariables == null ? "" : _serverVariables["HTTP_HOST"]); } set { _host = value; } }
        //private string _host;

        ///// <summary>
        ///// The URL path of the request causing this error
        ///// </summary>
        //public string Url { get { return _url ?? (_url = _serverVariables == null ? "" : _serverVariables["URL"]); } set { _url = value; } }
        //private string _url;

        /// <summary>
        /// The HTTP Method causing this error, e.g. GET or POST
        /// </summary>
        public string HTTPMethod { get { return _httpMethod ?? (_httpMethod = _serverVariables == null ? "" : _serverVariables["REQUEST_METHOD"]); } set { _httpMethod = value; } }
        private string _httpMethod;

        /// <summary>
        /// The IPAddress of the request causing this error
        /// </summary>
        public string IPAddress { get { return _ipAddress ?? (_ipAddress = _serverVariables == null ? "" : _serverVariables.GetRemoteIP()); } set { _ipAddress = value; } }
        private string _ipAddress;

        private StringBuilder GetPairs(NameValueCollection nvc)
        {
            var result = new StringBuilder();
            if (nvc == null)
                return result;

            for (int i = 0; i < nvc.Count; i++)
            {
                result.AppendFormat("{0}:{1},", nvc.GetKey(i), nvc.Get(i));
            }
            return result;
        }

        //public override string ToString()
        //{
        //    return string.Format("Url:{0}\r\nMachineName:{1}\r\nSource:{2}\r\nDetail:{3}\r\nStatusCode:{4}\r\nHTTPMethod:{5}\r\nIPAddress:{6}\r\nRequestHeaders {7}\r\nCookies {8}",
        //        AbsUrl, MachineName, Source, Detail, StatusCode.ToString(), HTTPMethod, IPAddress, GetPairs(_requestHeaders).ToString(), GetPairs(_cookies).ToString());
        //}

        public override string ToString()
        {
            return string.Format("Url:{0}\r\nMachineName:{1}\r\nSource:{2}{3}\r\nStatusCode:{4}\r\nHTTPMethod:{5}\r\nIPAddress:{6}\r\nRequestHeaders {7}\r\nCookies {8}\r\nQueryString {9}\r\nForm {10}",
                AbsUrl, MachineName, Source, "", StatusCode.ToString(), HTTPMethod, IPAddress, GetPairs(_requestHeaders).ToString(), GetPairs(_cookies).ToString(), GetPairs(_queryString).ToString(), GetPairs(_form).ToString());
        }
    }

    /// <summary>
    /// Serialization class in place of the NameValueCollection pairs
    /// </summary>
    /// <remarks>This exists because things like a querystring can havle multiple values, they are not a dictionary</remarks>
    public class NameValuePair
    {
        /// <summary>
        /// The name for this variable
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The value for this variable
        /// </summary>
        public string Value { get; set; }
    }

    static class HttpContextBaseExtensions
    {




        #region client ip
        /// <summary>
        /// When a client IP can't be determined
        /// </summary>
        public const string UnknownIP = "0.0.0.0";

        private static readonly Regex IPv4Regex = new Regex(@"\b([0-9]{1,3}\.){3}[0-9]{1,3}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// returns true if this is a private network IP  
        /// http://en.wikipedia.org/wiki/Private_network
        /// </summary>
        private static bool IsPrivateIP(string s)
        {
            return (s.StartsWith("192.168.") || s.StartsWith("10.") || s.StartsWith("127.0.0."));
        }
        /// <summary>
        /// retrieves the IP address of the current request -- handles proxies and private networks
        /// </summary>
        public static string GetRemoteIP(this NameValueCollection serverVariables)
        {
            var ip = serverVariables["REMOTE_ADDR"]; // could be a proxy -- beware
            var ipForwarded = serverVariables["HTTP_X_FORWARDED_FOR"];

            // check if we were forwarded from a proxy
            if (!string.IsNullOrEmpty(ipForwarded))
            {
                ipForwarded = IPv4Regex.Match(ipForwarded).Value;
                if (!string.IsNullOrEmpty(ipForwarded) && !IsPrivateIP(ipForwarded))
                    ip = ipForwarded;
            }

            return !string.IsNullOrEmpty(ip) ? ip : UnknownIP;
        }



        #endregion
    }

    internal static class Utils
    {

        readonly static DateTime START_TIME = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式精确到17位。
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimeTicks(DateTime time)
        {
            return (time - START_TIME).Ticks;
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式精确到10位。
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetUnixTime(DateTime time)
        {
            return (long)(time - START_TIME).TotalSeconds;
        }


        /// <summary>
        /// 将IPv4格式的字符串转换为int型表示
        /// </summary>
        /// <param name="strIPAddress">IPv4格式的字符</param>
        /// <returns></returns>
        public static long IPToNumber(string strIPAddress)
        {
            if (string.IsNullOrWhiteSpace(strIPAddress)) { return 0; }
            string[] arrayIP = strIPAddress.Split('.');
            long sip1 = long.Parse(arrayIP[0]);
            long sip2 = long.Parse(arrayIP[1]);
            long sip3 = long.Parse(arrayIP[2]);
            long sip4 = long.Parse(arrayIP[3]);

            return (sip1 << 24) + (sip2 << 16) + (sip3 << 8) + sip4;
        }

        /// <summary>
        /// 获取服务器IP
        /// </summary>
        /// <returns></returns>
        public static string GetServerIP()
        {
            string str = "127.0.0.1";
            try
            {
                string hostName = Dns.GetHostName();
                var hostEntity = Dns.GetHostEntry(hostName);
                var ipAddressList = hostEntity.AddressList;
                var ipAddress = ipAddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddress != null)
                {
                    str = ipAddress.ToString();
                }
                return str;
            }
            catch (Exception) { str = string.Empty; }
            return str;
        }


        ///// <summary>
        ///// 将int型表示的IP还原成正常IPv4格式。
        ///// </summary>
        ///// <param name="intIPAddress">int型表示的IP</param>
        ///// <returns></returns>
        //public static string NumberToIP(long intIPAddress)
        //{
        //    long tempIPAddress;
        //    //将目标整形数字intIPAddress转换为IP地址字符串
        //    //-1062731518 192.168.1.2 
        //    //-1062731517 192.168.1.3 
        //    if (intIPAddress >= 0)
        //    {
        //        tempIPAddress = intIPAddress;
        //    }
        //    else
        //    {
        //        tempIPAddress = intIPAddress + 1;
        //    }
        //    long s1 = tempIPAddress / 256 / 256 / 256;
        //    long s21 = s1 * 256 * 256 * 256;
        //    long s2 = (tempIPAddress - s21) / 256 / 256;
        //    long s31 = s2 * 256 * 256 + s21;
        //    long s3 = (tempIPAddress - s31) / 256;
        //    long s4 = tempIPAddress - s3 * 256 - s31;
        //    if (intIPAddress < 0)
        //    {
        //        s1 = 255 + s1;
        //        s2 = 255 + s2;
        //        s3 = 255 + s3;
        //        s4 = 255 + s4;
        //    }
        //    string strIPAddress = s1.ToString() + "." + s2.ToString() + "." + s3.ToString() + "." + s4.ToString();
        //    return strIPAddress;
        //}

    }
}
