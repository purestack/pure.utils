using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Pure.Utils
{

    public class HttpClientManage
    {

        private static HttpClient _httpClient;
        private static readonly object olock = new object();
        private static CookieContainer cookieContainer;
        static Uri baseUri = null;
        static string baseIPAddress = "";
        public static HttpClient GetOrCreateClient(string _baseIPAddress)
        {
            if (_httpClient == null)
            {
                lock (olock)
                {
                    baseIPAddress = _baseIPAddress;
                    cookieContainer = new CookieContainer();
                    HttpClientHandler httpClientHandler = new HttpClientHandler()
                    {
                        CookieContainer = cookieContainer,
                        AllowAutoRedirect = true,
                        UseCookies = true
                    };
                    baseUri = new Uri(_baseIPAddress, UriKind.Absolute);
                    _httpClient = new HttpClient(httpClientHandler) { BaseAddress = baseUri };
                    _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
                    TinyLogger.Instance.WriteLog("HttpClientManage Created HttpClient at " + DateTime.Now);
                }
            }

            return _httpClient;
        }


        public static void SetCookie(string cookies)
        {
            if (_httpClient == null)
            {
                _httpClient = GetOrCreateClient(baseIPAddress);
            }
            //_httpClient.DefaultRequestHeaders.Add("Host", "www.oschina.net");
            //_httpClient.DefaultRequestHeaders.Add("Method", "Post");
            //_httpClient.DefaultRequestHeaders.Add("KeepAlive", "false");   // HTTP KeepAlive设为false，防止HTTP连接保持
            //_httpClient.DefaultRequestHeaders.Add("UserAgent","Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
            if (_httpClient.DefaultRequestHeaders.Contains("Cookie"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");

            }

            _httpClient.DefaultRequestHeaders.Add("Cookie", cookies);


            foreach (var item in cookies.Split(';'))
            {
                cookieContainer.SetCookies(baseUri, item);

            }

        }

    }

    /// <summary>
    /// http请求类
    /// </summary>
    public class HttpHelper : IDisposable
    {

        private string _baseIPAddress;
        public bool EnableLog { get; private set; }
        public Stopwatch watch { get; private set; }
        private HttpClient _httpClient;

        /// <param name="ipaddress">请求的基础IP，例如：http://192.168.0.33:8080/ </param>
        public HttpHelper(string ipaddress = "", bool enableLog = true)
        {
            this.EnableLog = enableLog;
            this._baseIPAddress = ipaddress;
            if (enableLog == true)
            {
                watch = new Stopwatch();
            }

        }

        /// <summary>
        /// 初始化客户端加载器
        /// </summary>
        private void InitHttpClient()
        {
            //cookieContainer = new CookieContainer();
            //HttpClientHandler httpClientHandler = new HttpClientHandler()
            //{
            //    CookieContainer = cookieContainer,
            //    AllowAutoRedirect = true,
            //    UseCookies = true
            //};
            //baseUri = new Uri(_baseIPAddress, UriKind.Absolute);
            _httpClient = HttpClientManage.GetOrCreateClient(this._baseIPAddress); // new HttpClient(httpClientHandler) { BaseAddress = baseUri };
        }



        /// <summary>
        /// 创建带用户信息的请求客户端
        /// </summary>
        /// <param name="userName">用户账号</param>
        /// <param name="pwd">用户密码，当WebApi端不要求密码验证时，可传空串</param>
        /// <param name="uriString">The URI string.</param>
        public HttpHelper(string userName, string pwd = "", string uriString = "", bool enableLog = true)
            : this(uriString, true)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                _httpClient.DefaultRequestHeaders.Authorization = CreateBasicCredentials(userName, pwd);
            }
        }

        public void SetCookie(string cookies)
        {
            //_httpClient.DefaultRequestHeaders.Add("Host", "www.oschina.net");
            //_httpClient.DefaultRequestHeaders.Add("Method", "Post");
            //_httpClient.DefaultRequestHeaders.Add("KeepAlive", "false");   // HTTP KeepAlive设为false，防止HTTP连接保持
            //_httpClient.DefaultRequestHeaders.Add("UserAgent","Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
            //if (_httpClient.DefaultRequestHeaders.Contains("Cookie"))
            //{
            //    _httpClient.DefaultRequestHeaders.Remove("Cookie");

            //}

            //_httpClient.DefaultRequestHeaders.Add("Cookie", cookies);


            //foreach (var item in cookies.Split(';'))
            //{
            //    cookieContainer.SetCookies(baseUri, item);

            //}

            HttpClientManage.SetCookie(cookies);

        }

        /// <summary>
        /// Get请求数据
        /// <para>最终以url参数的方式提交</para>
        /// </summary>
        /// <param name="parameters">参数字典,可为空</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <returns></returns>
        public string Get(Dictionary<string, string> parameters, string requestUri)
        {

            if (parameters != null)
            {
                var strParam = string.Join("&", parameters.Select(o => o.Key + "=" + o.Value));
                requestUri = string.Concat(ConcatURL(requestUri), '?', strParam);
            }
            else
            {
                requestUri = ConcatURL(requestUri);
            }
            InitHttpClient();
            // requestUri = ConcutAbsUrl(requestUri);
            if (EnableLog == true)
            {
                if (watch.IsRunning)
                {
                    watch.Reset();
                }
                watch.Start();
            }

            var result = _httpClient.GetStringAsync(requestUri);
            if (EnableLog == true)
            {
                watch.Stop();
                TinyLogger.Instance.WriteLog("Elapsed Request for(" + requestUri + ") :" + watch.ElapsedMilliseconds + " ms");
            }

            return result.Result;
        }

        /// <summary>
        /// Get请求数据
        /// <para>最终以url参数的方式提交</para>
        /// </summary>
        /// <param name="parameters">参数字典</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <returns>实体对象</returns>
        public T Get<T>(Dictionary<string, string> parameters, string requestUri) where T : class
        {

            string jsonString = Get(parameters, requestUri);
            if (string.IsNullOrEmpty(jsonString))
                return null;

            var a = JsonHelper.Deserialize<T>(jsonString);

            return a;
        }

        /// <summary>
        /// 以json的方式Post数据 返回string类型
        /// <para>最终以json的方式放置在http体中</para>
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <returns></returns>
        public string Post(object entity, string requestUri)
        {


            string request = string.Empty;
            if (entity != null)
                request = JsonHelper.Serialize(entity);
            HttpContent httpContent = new StringContent(request);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return Post(requestUri, httpContent);
        }

        public T Post<T>(object entity, string requestUri) where T : class
        {
            string jsonString = Post(entity, requestUri);
            return JsonHelper.Deserialize<T>(jsonString);

        }

        /// <summary>
        /// 提交字典类型的数据
        /// <para>最终以formurlencode的方式放置在http体中</para>
        /// </summary>
        /// <returns>System.String.</returns>
        public string PostDicObj(Dictionary<string, object> para, string requestUri)
        {
            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach (var item in para)
            {
                if (item.Value != null)
                {
                    if (item.Value.GetType().Name.ToLower() != "string")
                    {
                        temp.Add(item.Key, JsonHelper.Serialize(item.Value));
                    }
                    else
                    {
                        temp.Add(item.Key, item.Value.ToString());
                    }
                }
                else
                {
                    temp.Add(item.Key, "");
                }
            }

            return PostDic(temp, requestUri);
        }
        public string PostDicObjJson(Dictionary<string, object> para, string requestUri)
        {
            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach (var item in para)
            {
                if (item.Value != null)
                {
                    if (item.Value.GetType().Name.ToLower() != "string")
                    {
                        temp.Add(item.Key, JsonHelper.Serialize(item.Value));
                    }
                    else
                    {
                        temp.Add(item.Key, item.Value.ToString());
                    }
                }
                else
                {
                    temp.Add(item.Key, "");
                }
            }

            return PostDicJson(temp, requestUri);
        }
        public T PostDicObj<T>(Dictionary<string, object> para, string requestUri) where T : class
        {
            string jsonString = PostDicObj(para, requestUri);
            return JsonHelper.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Post Dic数据
        /// <para>最终以formurlencode的方式放置在http体中</para>
        /// </summary>
        /// <returns>System.String.</returns>
        public string PostDic(Dictionary<string, string> temp, string requestUri)
        {
            HttpContent httpContent = new FormUrlEncodedContent(temp);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return Post(requestUri, httpContent);
        }

        public string PostDicJson(Dictionary<string, string> temp, string requestUri)
        {
            HttpContent httpContent = new FormUrlEncodedContent(temp);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return Post(requestUri, httpContent);
        }
        public T PostDic<T>(Dictionary<string, string> temp, string requestUri) where T : class
        {
            string jsonString = PostDic(temp, requestUri);
            return JsonHelper.Deserialize<T>(jsonString);
        }

        public string PostByte(byte[] bytes, string requestUrl)
        {
            HttpContent content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return Post(requestUrl, content);
        }

        private string Post(string requestUrl, HttpContent content)
        {
            InitHttpClient();
            if (EnableLog == true)
            {
                if (watch.IsRunning)
                {
                    watch.Reset();
                }
                watch.Start();
            }


            var result = _httpClient.PostAsync(ConcatURL(requestUrl), content);
            if (EnableLog == true)
            {
                watch.Stop();
                TinyLogger.Instance.WriteLog("Elapsed Request for(" + requestUrl + ") :" + watch.ElapsedMilliseconds + " ms");
            }
            var a = result.Result.Content.ReadAsStringAsync().Result;
            return a;
        }

        /// <summary>
        /// 把请求的URL相对路径组合成绝对路径
        /// </summary>
        private string ConcatURL(string requestUrl)
        {
            //var uri = new Uri(_httpClient.BaseAddress, requestUrl);
            return this._baseIPAddress + requestUrl;
        }

        private AuthenticationHeaderValue CreateBasicCredentials(string userName, string password)
        {
            string toEncode = userName + ":" + password;
            // The current HTTP specification says characters here are ISO-8859-1.
            // However, the draft specification for the next version of HTTP indicates this encoding is infrequently
            // used in practice and defines behavior only for ASCII.
            Encoding encoding = Encoding.GetEncoding("utf-8");
            byte[] toBase64 = encoding.GetBytes(toEncode);
            string parameter = Convert.ToBase64String(toBase64);

            return new AuthenticationHeaderValue("Basic", parameter);
        }

        public void Dispose()
        {
            //if (_httpClient != null)
            //{ 
            //    _httpClient.Dispose();
            //}
        }
    }
}
