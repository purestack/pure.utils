using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
 
using System;
using System.IO;
using System.Threading;
using System.Web;

namespace Pure.Utils
{
    /// <summary>
    /// 版 本 PureZero V8.2 敏捷开发框架 Copyright (c) 2012-2018
    /// 日 期：2017.03.07
    /// 描 述：文件下载类
    /// </summary>
    public class FileDownHelper
    {

        public FileDownHelper()
        { }
        /// <summary>
        /// 参数为虚拟路径
        /// </summary>
        public static string FileNameExtension(string FileName)
        {
            return Path.GetExtension(MapPathFile(FileName));
        }
        /// <summary>
        /// 获取物理地址
        /// </summary>
        public static string MapPathFile(string FileName)
        {
#if !NET45
            return Pure.Utils.HttpContext.HttpContextWrappers.Server.MapPath(FileName);

#else
            return HttpContext.Current.Server.MapPath(FileName);

            
#endif
        }
        /// <summary>
        /// 验证文件是否存在
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static bool FileExists(string FileName)
        {
            string destFileName = FileName;
            if (File.Exists(destFileName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 普通下载
        /// </summary>
        /// <param name="FileName">文件虚拟路径</param>
        ///  /// <param name="name">返回客户端名称</param>
        public static void DownLoadold(string FileName, string name)
        {
            string destFileName = FileName;
            if (File.Exists(destFileName))
            {
#if !NET45
                FileInfo fi = new FileInfo(destFileName);
                var response = Pure.Utils.HttpContext.HttpContextWrappers.Response;
                response.Clear();
                response.ClearHeaders();
                //  Pure.Utils.HttpContext.HttpContextWrappers.Response.Buffer = false;
                response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(name, System.Text.Encoding.UTF8));
                response.AddHeader("Content-Length", fi.Length.ToString());
                response.ContentType = "application/octet-stream";

 
                using (var stream = System.IO.File.OpenRead(destFileName))
                {
                    int length = (int)stream.Length;
                    var buffer = new byte[length];
                    stream.WriteAsync(buffer, 0, length);
                      response.Body.WriteAsync(buffer, 0, length);


                   // Pure.Utils.HttpContext.HttpContextWrappers.Response.StreamWrite(stream);

                    stream.Close();
                }
                //Pure.Utils.HttpContext.HttpContextWrappers.Response.Flush();
                //Pure.Utils.HttpContext.HttpContextWrappers.Response.End();


#else
FileInfo fi = new FileInfo(destFileName);
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.Buffer = false;
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(name, System.Text.Encoding.UTF8));
                HttpContext.Current.Response.AppendHeader("Content-Length", fi.Length.ToString());
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.WriteFile(destFileName);
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            
#endif

            }
        }
        /// <summary>
        /// 分块下载
        /// </summary>
        /// <param name="FileName">文件虚拟路径</param>
        public static void DownLoad(string FileName)
        {
            string filePath = MapPathFile(FileName);
            long chunkSize = 204800;             //指定块大小 
            byte[] buffer = new byte[chunkSize]; //建立一个200K的缓冲区 
            long dataToRead = 0;                 //已读的字节数   
            FileStream stream = null;


#if !NET45
            try
            {
                //打开文件   
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                dataToRead = stream.Length;

                //添加Http头   
                Pure.Utils.HttpContext.HttpContextWrappers.Response.ContentType = "application/octet-stream";
                Pure.Utils.HttpContext.HttpContextWrappers.Response.AddHeader("Content-Disposition", "attachement;filename=" + HttpUtility.UrlEncode(Path.GetFileName(filePath)));
                Pure.Utils.HttpContext.HttpContextWrappers.Response.AddHeader("Content-Length", dataToRead.ToString());
                 
                Pure.Utils.HttpContext.HttpContextWrappers.Response.StreamWrite(stream);
                Pure.Utils.HttpContext.HttpContextWrappers.Response.Flush();
                Pure.Utils.HttpContext.HttpContextWrappers.Response.Clear();
            }
            catch (Exception ex)
            {
                Pure.Utils.HttpContext.HttpContextWrappers.Response.Write("Error:" + ex.Message);
            }
            finally
            {
                if (stream != null) stream.Close();
                Pure.Utils.HttpContext.HttpContextWrappers.Response.End();
            }
#else
  try
            {
                //打开文件   
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                dataToRead = stream.Length;

                //添加Http头   
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachement;filename=" + HttpUtility.UrlEncode(Path.GetFileName(filePath)));
                HttpContext.Current.Response.AddHeader("Content-Length", dataToRead.ToString());

                while (dataToRead > 0)
                {
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        int length = stream.Read(buffer, 0, Convert.ToInt32(chunkSize));
                        HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);
                        HttpContext.Current.Response.Flush();
                        HttpContext.Current.Response.Clear();
                        dataToRead -= length;
                    }
                    else
                    {
                        dataToRead = -1; //防止client失去连接 
                    }
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("Error:" + ex.Message);
            }
            finally
            {
                if (stream != null) stream.Close();
                HttpContext.Current.Response.Close();
            }
            
#endif

        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filePath">物理地址</param>
        public static void DownLoadnew(string filePath)
        {
            long chunkSize = 204800;             //指定块大小 
            byte[] buffer = new byte[chunkSize]; //建立一个200K的缓冲区 
            long dataToRead = 0;                 //已读的字节数   
            FileStream stream = null;
#if !NET45

            try
            {
                //打开文件   
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                dataToRead = stream.Length;

                //添加Http头   
                Pure.Utils.HttpContext.HttpContextWrappers.Response.ContentType = "application/octet-stream";
                Pure.Utils.HttpContext.HttpContextWrappers.Response.AddHeader("Content-Disposition", "attachement;filename=" + HttpUtility.UrlEncode(Path.GetFileName(filePath)));
                Pure.Utils.HttpContext.HttpContextWrappers.Response.AddHeader("Content-Length", dataToRead.ToString());



                Pure.Utils.HttpContext.HttpContextWrappers.Response.StreamWrite(stream);
                Pure.Utils.HttpContext.HttpContextWrappers.Response.Flush();
                Pure.Utils.HttpContext.HttpContextWrappers.Response.Clear();
                 
            }
            catch (Exception ex)
            {
                Pure.Utils.HttpContext.HttpContextWrappers.Response.Write("Error:" + ex.Message);
            }
            finally
            {
                if (stream != null) stream.Close();
                Pure.Utils.HttpContext.HttpContextWrappers.Response.End();
            }
#else

           
            try
            {
                //打开文件   
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                dataToRead = stream.Length;

                //添加Http头   
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachement;filename=" + HttpUtility.UrlEncode(Path.GetFileName(filePath)));
                HttpContext.Current.Response.AddHeader("Content-Length", dataToRead.ToString());

                while (dataToRead > 0)
                {
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        int length = stream.Read(buffer, 0, Convert.ToInt32(chunkSize));
                        HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);
                        HttpContext.Current.Response.Flush();
                        HttpContext.Current.Response.Clear();
                        dataToRead -= length;
                    }
                    else
                    {
                        dataToRead = -1; //防止client失去连接 
                    }
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("Error:" + ex.Message);
            }
            finally
            {
                if (stream != null) stream.Close();
                HttpContext.Current.Response.Close();
            } 
#endif

        }
        /// <summary>
        ///  输出硬盘文件，提供下载 支持大文件、续传、速度限制、资源占用小
        /// </summary>
        /// <param name="_Request">Page.Request对象</param>
        /// <param name="_Response">Page.Response对象</param>
        /// <param name="_fileName">下载文件名</param>
        /// <param name="_fullPath">带文件名下载路径</param>
        /// <param name="_speed">每秒允许下载的字节数</param>
        /// <returns>返回是否成功</returns>
        //---------------------------------------------------------------------
        //调用：
        // string FullPath=Server.MapPath("count.txt");
        // ResponseFile(this.Request,this.Response,"count.txt",FullPath,100);
        //---------------------------------------------------------------------

#if !NET45
        public static bool ResponseFile(HttpRequest _Request, HttpResponse _Response, string _fileName, string _fullPath, long _speed)
        {
            try
            {
                FileStream myFile = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(myFile);
                try
                {
                    _Response.Headers.Add("Accept-Ranges", "bytes");
                  //  _Response.Buffer = false;

                    long fileLength = myFile.Length;
                    long startBytes = 0;
                    int pack = 10240;  //10K bytes
                    int sleep = (int)Math.Floor((double)(1000 * pack / _speed)) + 1;

                    if (_Request.Headers["Range"] != StringValues.Empty)
                    {
                        _Response.StatusCode = 206;
                        string[] range = _Request.Headers["Range"].ToString().Split(new char[] { '=', '-' });
                        startBytes = Convert.ToInt64(range[1]);
                    }
                    _Response.Headers.Add("Content-Length", (fileLength - startBytes).ToString());
                    if (startBytes != 0)
                    {
                        _Response.Headers.Add("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                    }

                    _Response.Headers.Add("Connection", "Keep-Alive");
                    _Response.ContentType = "application/octet-stream";
                    _Response.Headers.Add("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(_fileName, System.Text.Encoding.UTF8));

                    br.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                    int maxCount = (int)Math.Floor((double)((fileLength - startBytes) / pack)) + 1;

                    for (int i = 0; i < maxCount; i++)
                    {
                        var data = br.ReadBytes(pack);
                        _Response.Body.WriteAsync(data, 0 , data.Length);
                        Thread.Sleep(sleep);

                        //if (_Response.IsClientConnected)
                        //{
                        //    _Response.BinaryWrite(br.ReadBytes(pack));
                        //    Thread.Sleep(sleep);
                        //}
                        //else
                        //{
                        //    i = maxCount;
                        //}
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    br.Close();
                    myFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
#else
           public static bool ResponseFile(HttpRequest _Request, HttpResponse _Response, string _fileName, string _fullPath, long _speed)
        {
            try
            {
                FileStream myFile = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(myFile);
                try
                {
                    _Response.AddHeader("Accept-Ranges", "bytes");
                    _Response.Buffer = false;

                    long fileLength = myFile.Length;
                    long startBytes = 0;
                    int pack = 10240;  //10K bytes
                    int sleep = (int)Math.Floor((double)(1000 * pack / _speed)) + 1;

                    if (_Request.Headers["Range"] != null)
                    {
                        _Response.StatusCode = 206;
                        string[] range = _Request.Headers["Range"].Split(new char[] { '=', '-' });
                        startBytes = Convert.ToInt64(range[1]);
                    }
                    _Response.AddHeader("Content-Length", (fileLength - startBytes).ToString());
                    if (startBytes != 0)
                    {
                        _Response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                    }

                    _Response.AddHeader("Connection", "Keep-Alive");
                    _Response.ContentType = "application/octet-stream";
                    _Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(_fileName, System.Text.Encoding.UTF8));

                    br.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                    int maxCount = (int)Math.Floor((double)((fileLength - startBytes) / pack)) + 1;

                    for (int i = 0; i < maxCount; i++)
                    {
                        if (_Response.IsClientConnected)
                        {
                            _Response.BinaryWrite(br.ReadBytes(pack));
                            Thread.Sleep(sleep);
                        }
                        else
                        {
                            i = maxCount;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    br.Close();
                    myFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
            
#endif

    }
}
