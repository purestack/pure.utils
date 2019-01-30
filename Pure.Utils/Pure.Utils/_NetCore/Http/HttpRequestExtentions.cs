using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Http.Internal;
namespace Pure.Utils
{
    public static class HttpRequestExtentions
    { 

        private const string RequestedWithHeader = "X-Requested-With";
        private const string XmlHttpRequest = "XMLHttpRequest";

        /// <summary>
        /// 判断是否Ajax请求
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is an AJAX request; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="request"/> parameter is <c>null</c>.</exception>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Headers != null)
            {
                return request.Headers[RequestedWithHeader] == XmlHttpRequest;
            }


          

            return false;
        }

        /// <summary>
        /// 是否本地请求
        /// originator was 127.0.0.1.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is a local request; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="request"/> parameter is <c>null</c>.</exception>
        public static bool IsLocalRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                string str = connection.RemoteIpAddress.ToString();
                if (str == "127.0.0.1" || str == "::1" || str == "localhost")
                {
                    return true;
                }
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                else
                {
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }



        public static string GetClientIpAddress(this HttpRequest request)
        {
            return request.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        public static string GetConnectionId(this HttpRequest request)
        {
            return request.HttpContext.Connection.Id;
        }

        public static string GetRequestId(this HttpRequest request)
        {
            return request.Headers.FirstOrDefault("X-Request-ID");
        }

        public static string GetUserAgent(this HttpRequest request)
        {
            return request.Headers.FirstOrDefault("User-Agent");
        }


        public static string GetUserHostAddress(this HttpRequest request)
        {
            return   request.Headers["X-Original-For"];
        }

        public static string GetRawUrl(this HttpRequest request)
        {
            return request.GetDisplayUrl(); 
        }
        public static bool HasFormParams(this HttpRequest request)
        {
            return request.HasFormContentType;//request.Method == HttpMethods.Post
        }

        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            request.EnableRewind();
            request.Body.Position = 0;
            if (encoding is null)
                encoding = Encoding.UTF8;

            using (var reader = new StreamReader(request.Body, encoding))
                return await reader.ReadToEndAsync();
        }
        public static async Task<byte[]> GetRawBodyBytesAsync(this HttpRequest request)
        {
            request.EnableRewind();
            request.Body.Position = 0;
            using (var ms = new MemoryStream(2048))
            {
                await request.Body.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        public static  string GetRawBodyString(this HttpRequest request, Encoding encoding = null)
        { 
            if (encoding is null)
                encoding = Encoding.UTF8;
            request.EnableRewind();
            request.Body.Position = 0;
            using (var reader = new StreamReader(request.Body, encoding))
                return reader.ReadToEnd();
        }
        public static byte[] GetRawBodyBytes(this HttpRequest request)
        {
            request.EnableRewind();
            request.Body.Position = 0;
            using (var ms = new MemoryStream(2048))
            {
                 request.Body.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
