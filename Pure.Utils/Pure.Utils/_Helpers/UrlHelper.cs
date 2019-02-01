using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Pure.Utils
{
    /// <summary>
    /// URl帮助类
    /// </summary>
    public static class UrlHelper
    {

        public static string EnsureAbsoluteUrl(string url, string faviconUrl)
        {
            if (string.IsNullOrEmpty(faviconUrl)) return string.Empty;

            if (faviconUrl.StartsWith("//"))
            {
                return faviconUrl.Replace("//", "https://");
            }

            if (!IsAbsoluteUrl(faviconUrl))
            {
                if (faviconUrl.StartsWith("./"))
                {
                    if (!url.EndsWith("/"))
                    {
                        url = url + "/";
                    }

                    faviconUrl = faviconUrl.Replace("./", string.Empty);
                }

                if (faviconUrl.StartsWith("/"))
                {
                    return new Uri(GetUri(url), faviconUrl).AbsoluteUri;
                }

                return new Uri(faviconUrl, UriKind.Relative).ToAbsolute(url);
            }

            return faviconUrl;
        }

        public static string ToAbsolute(this Uri uri, string baseUrl)
        {
            if (uri == null) return string.Empty;

            return uri.ToAbsolute(GetUri(baseUrl));
        }

        public static string ToAbsolute(this Uri uri, Uri baseUri)
        {
            if (uri == null) return string.Empty;

            var relative = uri.ToRelative();

            if (Uri.TryCreate(baseUri, relative, out var absolute))
            {
                return absolute.ToString();
            }

            return uri.IsAbsoluteUri ? uri.ToString() : null;
        }

        public static string ToRelative(this Uri uri)
        {
            if (uri == null) return string.Empty;

            return uri.IsAbsoluteUri ? uri.PathAndQuery : uri.OriginalString;
        }

        // Check if the url exists by checking for a 200 OK response
        public static bool UrlExists(string url)
        {
            try
            {
                var webRequest = WebRequest.Create(url);
                webRequest.Method = "HEAD";
                var webResponse = (HttpWebResponse)webRequest.GetResponse();
                return webResponse.StatusCode == HttpStatusCode.OK && webResponse.ContentLength > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Make sure string url has correct format and a http as scheme if missing
        public static Uri GetUri(this string url)
            => new UriBuilder(url).Uri;

        // Check if the url is absolute eg. starts with http
        private static bool IsAbsoluteUrl(string url)
            => url.StartsWith("http://") || url.StartsWith("https://");
        /// <summary>
        /// 在URL后面追加参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetAppendedQueryString(string url, string key, string value)
        {
            if (url.Contains("?"))
            {
                url = string.Format("{0}&{1}={2}", url, key, value);
            }
            else
            {
                url = string.Format("{0}?{1}={2}", url, key, value);
            }

            return url;
        }

        public static string RemoveParameter(string url, string key)
        {

            url = url.ToLower();
            key = key.ToLower();
            if (!url.Contains(key + "=")) return url;

            Uri uri = new Uri(url);
            NameValueCollection collection = HttpUtility.ParseQueryString(uri.Query);
            if (collection.Count == 0) return url;

            var val = collection[key];
            string fragmentToRemove = string.Format("{0}={1}", key, val);

            String result = url.ToLower().Replace("&" + fragmentToRemove, string.Empty).Replace("?" + fragmentToRemove, string.Empty);
            return result;
        }

        ///// <summary>
        ///// 根据URL的相对地址获取决定路径
        ///// <para>eg: /Home/About ==>http://192.168.0.1/Home/About</para>
        ///// </summary>
        ///// <returns>System.String.</returns>
        //public static string GetAbsolutePathForRelativePath(string relativePath)
        //{
        //    HttpRequest Request = HttpContext.Current.Request;
        //    string returnUrl = string.Format("{0}{1}", Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, string.Empty), VirtualPathUtility.ToAbsolute(relativePath));
        //    return returnUrl;
        //}
    }
}
