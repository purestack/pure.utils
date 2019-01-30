using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pure.Utils
{
    /// <summary>
    /// 系统错误信息
    /// </summary>
    public class ErrorMessage
    {
        public ErrorMessage()
        {

        }
        public ErrorMessage(Microsoft.AspNetCore.Http.HttpContext context, Exception ex, string type = "ERROR")
        {
            if (ex != null)
            {
                MsgType = ex.GetType().Name;
                Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                StackTrace = ex.StackTrace;
                Source = ex.Source;
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (ex.TargetSite != null)
                {
                    Assembly = ex.TargetSite.Module.Assembly.FullName;
                    Method = ex.TargetSite.Name;
                }
            }
           
            
            Type = type;

            DotNetVersion = Environment.Version.Major + "." + Environment.Version.Minor + "." + Environment.Version.Build + "." + Environment.Version.Revision;
            DotNetBit = (Environment.Is64BitProcess ? "64" : "32") + "位";
            OSVersion = Environment.OSVersion.ToString();
            CPUCount = Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS");
            CPUType = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            OSBit = (Environment.Is64BitOperatingSystem ? "64" : "32") + "位";
            MachineName = Environment.MachineName;

           // var context = HttpContext.Current;
            if (context != null)
            {
                var request = context.Request;
                IP = GetIpAddr(request) + ":" + request.GetUrl().Port;
                IISVersion = request.GetServerVariables()["SERVER_SOFTWARE"];
                UserAgent = request.GetUserAgent();
                Path = request.Path;
                AbsUrl = request.GetUrl().ToString();
                HttpMethod = request.Method;

                if (ex != null)
                {
                    StatusCode = 500;
                }
                //var httpException = ex as HttpException;
                //if (httpException != null)
                //{
                //    StatusCode = httpException.GetHttpCode();
                //}
                //else
                //{
                //    StatusCode = context.Response.StatusCode;
                //}
                SetContextProperties((context));
            }
          
        }
         

        private NameValueCollection _serverVariables;
        private NameValueCollection _queryString;
        private NameValueCollection _form;
        private NameValueCollection _cookies;
        private NameValueCollection _requestHeaders;
        internal const string CollectionErrorKey = "CollectionFetchError";

        private void SetContextProperties(Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (context == null) return;

            var request = context.Request;
            AbsUrl = request.GetUrl().ToString();
            Func<Func<HttpRequest , IQueryCollection>, NameValueCollection> tryGetCollection = getter =>
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
                catch ( Exception e)
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
            Func<Func<HttpRequest, IHeaderDictionary>, NameValueCollection>  tryGetCollectionServerVars = getter =>
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
        private StringBuilder GetPairs(NameValueCollection nvc)
        {
            var result = new StringBuilder();
            if (nvc == null)
                return result;

            for (int i = 0; i < nvc.Count; i++)
            {
                result.AppendFormat("{0}:{1},\r\n", nvc.GetKey(i), nvc.Get(i));
            }
            return result;
        }

        private string __QueryString = "";
        public string QueryString
        {
            get{
                if (string.IsNullOrEmpty(__QueryString))
                {
                    __QueryString = GetPairs(_queryString).ToString();

                }
                return __QueryString;
            }
            set { __QueryString = value; }
        }
        private string __RequestHeaders = "";
        public string RequestHeaders
        {
            get
            {
                if (string.IsNullOrEmpty(__RequestHeaders))
                {
                    __RequestHeaders = GetPairs(_requestHeaders).ToString();
                }
                return __RequestHeaders;
            }
            set { __RequestHeaders = value; }
        }

        private string __form = "";
        public string Form
        {
            get
            {
                if (string.IsNullOrEmpty(__form))
                {
                    __form = GetPairs(_form).ToString();

                }
                return __form;
            }
            set { __form = value; }
        }

        private string __cookies = "";
        public string Cookies
        {
            get
            {
                if (string.IsNullOrEmpty(__cookies))
                {
                    __cookies = GetPairs(_cookies).ToString();

                }
                return __cookies;
            }
            set { __cookies = value; }
        }


        private string __serverVariables = "";
        public string ServerVariables
        {
            get
            {
                if (string.IsNullOrEmpty(__serverVariables))
                {
                    __serverVariables = GetPairs(_serverVariables).ToString();

                }
                return __serverVariables;
            }
            set { __serverVariables = value; }
        }


        /// <summary>
        /// 绝对地址
        /// </summary>
        public string AbsUrl { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// 机器名
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 程序集名称
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// 异常参数
        /// </summary>
        public string ActionArguments { get; set; }

        /// <summary>
        /// 请求类型
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// 异常堆栈
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// 异常源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 服务器IP 端口
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 客户端浏览器标识
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// .NET解释引擎版本
        /// </summary>
        public string DotNetVersion { get; set; }

        /// <summary>
        ///  应用程序池位数
        /// </summary>
        public string DotNetBit { get; set; }


        /// <summary>
        /// 操作系统类型
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// 操作系统位数
        /// </summary>
        public string OSBit { get; set; }

        /// <summary>
        /// CPU个数
        /// </summary>
        public string CPUCount { get; set; }

        /// <summary>
        /// CPU类型
        /// </summary>
        public string CPUType { get; set; }

        /// <summary>
        /// IIS版本
        /// </summary>
        public string IISVersion { get; set; }

        /// <summary>
        /// 请求地址类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 是否显示异常界面
        /// </summary>
        public bool ShowException { get; set; }

        /// <summary>
        /// 异常发生时间
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// 异常发生方法
        /// </summary>
        public string Method { get; set; }


        private static string GetIpAddr(HttpRequest request)
        {
            //HTTP_X_FORWARDED_FOR
            var serverVars = request.GetServerVariables();
            string ipAddress = serverVars["x-forwarded-for"];
            if (!IsEffectiveIP(ipAddress))
            {
                ipAddress = serverVars["Proxy-Client-IP"];
            }
            if (!IsEffectiveIP(ipAddress))
            {
                ipAddress = serverVars["WL-Proxy-Client-IP"];
            }
            if (!IsEffectiveIP(ipAddress))
            {
                ipAddress = serverVars["Remote_Addr"];
                if (ipAddress.Equals("127.0.0.1") || ipAddress.Equals("::1"))
                {
                    // 根据网卡取本机配置的IP
                    IPAddress[] AddressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    foreach (IPAddress _IPAddress in AddressList)
                    {
                        if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                        {
                            ipAddress = _IPAddress.ToString();
                            break;
                        }
                    }
                }
            }
            // 对于通过多个代理的情况，第一个IP为客户端真实IP,多个IP按照','分割
            if (ipAddress != null && ipAddress.Length > 15)
            {
                if (ipAddress.IndexOf(",") > 0)
                {
                    ipAddress = ipAddress.Substring(0, ipAddress.IndexOf(","));
                }
            }
            return ipAddress;
        }

        /// <summary>
        /// 是否有效IP地址
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>bool</returns>
        private static bool IsEffectiveIP(string ipAddress)
        {
            return !(string.IsNullOrEmpty(ipAddress) || "unknown".Equals(ipAddress, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return string.Format("Url:{0}\r\nMachineName:{1}\r\nSource:{2}{3}\r\nStatusCode:{4}\r\nHTTPMethod:{5}\r\nIPAddress:{6}\r\nRequestHeaders {7}\r\nCookies {8}\r\nQueryString {9}\r\nForm {10}",
                AbsUrl, MachineName, Source, "", StatusCode.ToString(), HttpMethod, IP, GetPairs(_requestHeaders).ToString(), GetPairs(_cookies).ToString(), GetPairs(_queryString).ToString(), GetPairs(_form).ToString());
        }
    }
}
