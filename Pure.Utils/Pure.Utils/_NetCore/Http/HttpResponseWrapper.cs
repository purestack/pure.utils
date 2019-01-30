using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Pure.Utils
{
    public class HttpResponseWrapper
    {
        bool isEnd = false;
        Microsoft.AspNetCore.Http.HttpContext context;
        Microsoft.AspNetCore.Http.HttpResponse response => context.Response;
        HttpCookieCollection cookieCollection;
        public HttpResponseWrapper(Microsoft.AspNetCore.Http.HttpContext context)
        {
            this.context = context;
            cookieCollection = new HttpCookieCollection(context);
        }

        #region 兼容Web
        public void End()
        {
            isEnd = true;
            //context.Abort();
        }

        public string Charset
        {
            get
            {
                string ct = ContentType;
                int i = ct.IndexOf("charset=");
                if (i > -1)
                {
                    return ct.Substring(i + 8);
                }

                return "";
            }
            set
            {
                //Headers are read-only, response has already started
                if (HasStarted) { return; }
                string ct = ContentType;
                if (string.IsNullOrEmpty(ct))
                {
                    ContentType = "charset=" + value;
                }
                else if (!ct.Contains("charset="))
                {
                    ContentType = ct.TrimEnd(';') + "; charset=" + value;
                }
            }
        }
        public string ContentType
        {
            get => response.ContentType ?? "";
            set
            {
                if (value.Contains("charset") || string.IsNullOrEmpty(response.ContentType))
                {
                    response.ContentType = value;
                }
                else
                {
                    string[] items = response.ContentType.Split(';');
                    if (items.Length == 2)
                    {
                        response.ContentType = value + ";" + items[1];
                    }
                    else if (response.ContentType.Contains("charset"))//只有charset
                    {
                        response.ContentType = value + ";" + items[0];
                    }
                    else
                    {
                        response.ContentType = value;
                    }
                }


            }
        }
        public Stream Filter { get => response.Body; set => response.Body = value; }

        public void AppendHeader(string key, string value)
        {
            response.Headers.Append(key, value);
        }
        public void AddHeader(string key, string value)
        {
            response.Headers.Remove(key);
            response.Headers.Add(key, value);
        }
        //public bool Buffer { get; set; }
        //public int Expires { get; set; }
        //public DateTime ExpiresAbsolute { get; set; }
        public string CacheControl
        {
            get => Headers["Cache-Control"];
            set => AppendHeader("Cache-Control", value);
        }
        public void BinaryWrite(byte[] data)
        {
            // response.Body = new MemoryStream(data);
            if (!isEnd)
            {
                response.Body.WriteAsync(data, 0, data.Length);
            }
            //response.Body.Flush();
            //response.SendFileAsync()
        }
        public void StreamWrite(Stream st)
        {
            // response.Body = new MemoryStream(data);
            if (!isEnd)
            {
                st.CopyToAsync(response.Body);
            }
            //response.Body.Flush();
            //response.SendFileAsync()
        }
        public void Clear() { response.Clear(); }
        public void ClearHeaders() { response.Headers.Clear(); }
        public void Flush() { response.Body.FlushAsync(); }
        #endregion


        public HttpCookieCollection Cookies => cookieCollection;

        public int StatusCode
        {
            get => response.StatusCode; set
            {
                if (!response.HasStarted)
                {
                    response.StatusCode = value;
                }
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                NameValueCollection nvc = new NameValueCollection();
                if (response.Headers != null && response.Headers.Keys.Count > 0)
                {
                    foreach (string key in response.Headers.Keys)
                    {
                        nvc.Add(key, response.Headers[key].ToString());
                    }
                }
                return nvc;
            }
        }

        public Stream Body { get => response.Body; set => response.Body = value; }
        public long? ContentLength { get => response.ContentLength; set => response.ContentLength = value; }


        public bool HasStarted => response.HasStarted;



        public void OnCompleted(Func<object, Task> callback, object state)
        {
            response.OnCompleted(callback, state);
        }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            response.OnStarting(callback, state);
        }
        /// <summary>
        /// 302 跳转
        /// </summary>
        /// <param name="location"></param>
        public void Redirect(string location)
        {
            Redirect(location, false);
        }
        /// <summary>
        /// 跳转
        /// </summary>
        /// <param name="location"></param>
        /// <param name="permanent">true 301跳转，默认false 是302跳转</param>
        public void Redirect(string location, bool permanent)
        {
            response.Redirect(location, permanent);
            response.WriteAsync("");
        }
        public void Write(string text)
        {
            if (!isEnd)
            {
                response.WriteAsync(text);
            }
        }

        /// <summary>
        /// DownloadBigFile用于下载大文件，循环读取大文件的内容到服务器内存，然后发送给客户端浏览器
        /// </summary>
        public void WriteFile(string filePath, string ContentType, int bufferSize = 1024)
        {
            //var filePath = @"D:\Download\测试文档.xlsx";//要下载的文件地址，这个文件会被分成片段，通过循环逐步读取到ASP.NET Core中，然后发送给客户端浏览器
            var fileName = Path.GetFileName(filePath);//测试文档.xlsx

            //int bufferSize = 1024;//这就是ASP.NET Core循环读取下载文件的缓存大小，这里我们设置为了1024字节，也就是说ASP.NET Core每次会从下载文件中读取1024字节的内容到服务器内存中，然后发送到客户端浏览器，这样避免了一次将整个下载文件都加载到服务器内存中，导致服务器崩溃

            response.ContentType = ContentType;// "application/vnd.ms-excel";//由于我们下载的是一个Excel文件，所以设置ContentType为application/vnd.ms-excel

            var contentDisposition = "attachment;" + "filename=" + HttpUtility.UrlEncode(fileName);//在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名
            response.Headers.Append("Content-Disposition", new string[] { contentDisposition });



            //使用FileStream开始循环读取要下载文件的内容
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (response.Body)//调用Response.Body.Dispose()并不会关闭客户端浏览器到ASP.NET Core服务器的连接，之后还可以继续往Response.Body中写入数据
                {
                    long contentLength = fs.Length;//获取下载文件的大小
                    response.Headers.Append("Content-Length", contentLength.ToString());

                    response.ContentLength = contentLength;//在Response的Header中设置下载文件的大小，这样客户端浏览器才能正确显示下载的进度

                    byte[] buffer;
                    long hasRead = 0;//变量hasRead用于记录已经发送了多少字节的数据到客户端浏览器

                    //如果hasRead小于contentLength，说明下载文件还没读取完毕，继续循环读取下载文件的内容，并发送到客户端浏览器
                    while (hasRead < contentLength)
                    {
                        //HttpContext.RequestAborted.IsCancellationRequested可用于检测客户端浏览器和ASP.NET Core服务器之间的连接状态，如果HttpContext.RequestAborted.IsCancellationRequested返回true，说明客户端浏览器中断了连接
                        if (response.HttpContext.RequestAborted.IsCancellationRequested)
                        {
                            //如果客户端浏览器中断了到ASP.NET Core服务器的连接，这里应该立刻break，取消下载文件的读取和发送，避免服务器耗费资源
                            break;
                        }

                        buffer = new byte[bufferSize];

                        int currentRead = fs.Read(buffer, 0, bufferSize);//从下载文件中读取bufferSize(1024字节)大小的内容到服务器内存中

                        response.Body.WriteAsync(buffer, 0, currentRead);//发送读取的内容数据到客户端浏览器
                        response.Body.FlushAsync();//注意每次Write后，要及时调用Flush方法，及时释放服务器内存空间

                        hasRead += currentRead;//更新已经发送到客户端浏览器的字节数
                    }
                }
            }

            return ;
        }

    }
}