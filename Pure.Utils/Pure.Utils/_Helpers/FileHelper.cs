
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Pure.Utils
{
    /// <summary>
    /// This class provides various file helper methods.
    /// </summary>      
    public class FileHelper
    {

        public static MemoryStream ConvertToStream(string str) {

            byte[] array = Encoding.UTF8.GetBytes(str);
            MemoryStream stream = new MemoryStream(array);             //convert stream 2 string      
            //StreamReader reader = new StreamReader(stream);

            return stream;
        }
        /// <summary>  
        /// 读取文件，返回相应字符串  
        /// </summary>  
        /// <param name="fileName">文件路径</param>  
        /// <returns>返回文件内容</returns>  
        public static string ReadFile(string fileName, Encoding encoding = null)
        {
            StringBuilder str = new StringBuilder();
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            using (FileStream fs = File.OpenRead(fileName))
            {
                long left = fs.Length;
                int maxLength = 100;//每次读取的最大长度  
                int start = 0;//起始位置  
                int num = 0;//已读取长度  
                while (left > 0)
                {
                    byte[] buffer = new byte[maxLength];//缓存读取结果  
                    char[] cbuffer = new char[maxLength];
                    fs.Position = start;//读取开始的位置  
                    num = 0;
                    if (left < maxLength)
                    {
                        num = fs.Read(buffer, 0, Convert.ToInt32(left));
                    }
                    else
                    {
                        num = fs.Read(buffer, 0, maxLength);
                    }
                    if (num == 0)
                    {
                        break;
                    }
                    start += num;
                    left -= num;
                    str = str.Append(encoding.GetString(buffer));
                }
            }
            return str.ToString();
        }
        public static string ReadAllContent(string filepath, Encoding encoding = null) {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            string str = File.ReadAllText(filepath, encoding);
            return str;
        }
        /// <summary>
        /// Gets the orginal extension from a renamed extension. e.g. file.xml.config return .xml instead of .config. file.xml returns .xml.
        /// </summary>
        /// <param name="path">/config/users.csv.config</param>
        /// <param name="appendedExtension">The extra extension appended to the file. e.g. ".config"</param>
        /// <returns>String with original extension.</returns>
        public static string GetOriginalExtension(string path, string appendedExtension)
        {
            // Example /configfiles/users.csv.config
            FileInfo file = new FileInfo(path);
            string extension = file.Extension.ToLower();

            // None supplied ?
            if (string.IsNullOrEmpty(appendedExtension))
                return extension;

            // Now check that file ends w/ the extra extension
            appendedExtension = appendedExtension.ToLower();

            if (string.Compare(extension, appendedExtension, true) != 0)
                return extension;

            // Now get .csv from users.csv.config
            path = file.Name.Substring(0, file.Name.LastIndexOf(appendedExtension, StringComparison.InvariantCultureIgnoreCase));
            file = new FileInfo(path);
            return file.Extension.ToLower();
        }


        /// <summary>
        /// Prepend some text to a file.
        /// </summary>
        /// <param name="text">Text to prepend to a file.</param>
        /// <param name="file">File where text will be prepended.</param>
        public static void PrependText(string text, FileInfo file)
        {
            string content = File.ReadAllText(file.FullName);
            content = text + content;
            File.WriteAllText(file.FullName, content);
        }


        /// <summary>
        /// Get the file version information.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns>String with the file version.</returns>
        public static string GetVersion(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            // Get the file version for the notepad.
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return versionInfo.FileVersion;
        }


        /// <summary>
        /// Get file size in bytes.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns>File size in bytes.</returns>
        public static int GetSizeInBytes(string filePath)
        {
            FileInfo f = new FileInfo(filePath);
            return (int)f.Length;
        }


        /// <summary>
        /// Get file size in kilobytes.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns>File size in kilobytes.</returns>
        public static int GetSizeInKilo(string filePath)
        {
            FileInfo f = new FileInfo(filePath);
            float size = f.Length / 1000;
            return (int)size;
        }


        /// <summary>
        /// Get file size in megs.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns>File size in megabytes.</returns>
        public static int GetSizeInMegs(string filePath)
        {
            FileInfo f = new FileInfo(filePath);
            float size = f.Length / 1000000;
            return (int)size;
        }




        /// <summary>
        /// 文件后缀名
        /// </summary>
        public enum Extension
        {
            doc = 1,
            pdf = 2,
            xls = 3,
            rar = 4,
            jpg = 5,
            jpeg = 6,
            gif = 7,
            bmp = 8,
            html = 9,
            txt = 10,
            js = 11,
            css = 12,
            xml = 13,
            docx = 14,
            pptx = 15,
            xlsx = 16,
            png = 17,
            ppt = 18
        }

        /// <summary>
        /// 获取文件后缀名的mime类型
        /// </summary>
        /// <param name="FileExtension">文件后缀名</param>
        /// <returns></returns>
        public static string GetContentTypeByExtension(string FileExtension)
        {
            string _type = string.Empty;
            FileExtension = FileExtension.ToLower();
            switch (FileExtension)
            {
                case "doc":
                    _type = "application/msword";
                    break;
                case "pdf":
                    _type = "application/pdf";
                    break;
                case "xls":
                    _type = "application/vnd.ms-excel";
                    break;
                case "ppt":
                    _type = "application/vnd.ms-powerpoint";
                    break;
                case "rar":
                    _type = "application/zip";
                    break;
                case "zip":
                    _type = "application/zip";
                    break;
                case "jpg":
                    _type = "image/jpeg";
                    break;
                case "jpeg":
                    _type = "image/jpeg";
                    break;
                case "gif":
                    _type = "image/gif";
                    break;
                case "bmp":
                    _type = "application/x-bmp";
                    break;
                case "html":
                    _type = "text/html";
                    break;
                case "txt":
                    _type = "text/plain";
                    break;
                case "js":
                    _type = "application/x-javascript";
                    break;
                case "css":
                    _type = "text/css";
                    break;
                case "xml":
                    _type = "text/xml";
                    break;
                case "png":
                    _type = "image/png";
                    break;
                case "docx": _type = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
                case "pptx": _type = "application/vnd.openxmlformats-officedocument.presentationml.presentation"; break;
                case "xlsx": _type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
            }

            return _type;
        }

        /// <summary>
        /// 图片的后缀名
        /// </summary>
        public static IList<string> PictureExtension
        {
            get
            {
                IList<string> list = new List<string>();
                list.Add(Extension.jpeg.ToString());
                list.Add(Extension.jpg.ToString());
                list.Add(Extension.gif.ToString());
                list.Add(Extension.bmp.ToString());
                list.Add(Extension.png.ToString());
                return list;
            }
        }

        /// <summary>
        /// 是否为图片
        /// </summary>
        /// <param name="FileExtension"></param>
        /// <returns></returns>
        public static bool IsPicture(string FileExtension)
        {
            return PictureExtension.Contains(FileExtension.ToLower().Trim());
        }

        /// <summary>
        /// 显示文件大小
        /// </summary>
        /// <param name="fileSize">单位KB</param>
        /// <returns></returns>
        public static string ShowFileSizeText(double KBFileSize)
        {
            if (KBFileSize < 0)
                return "0K";
            string showFormat = string.Empty;
            if ((KBFileSize / 1024) >= 1)
            {
                if ((KBFileSize / 1048576) >= 1) //1048576 = 1024 * 1024
                {
                    if (KBFileSize % 1048576 == 0)
                    {
                        showFormat = (KBFileSize / 1048576.00).ToString() + "G";
                    }
                    else
                    {
                        showFormat = (KBFileSize / 1048576.00).ToString("F2") + "G";
                    }
                }
                else
                {
                    if (KBFileSize % 1024 == 0)
                    {
                        showFormat = (KBFileSize / 1024.00).ToString() + "M";
                    }
                    else
                    {
                        showFormat = (KBFileSize / 1024.00).ToString("F2") + "M";
                    }
                }
            }
            else
            {
                showFormat = KBFileSize.ToString("F2") + "K";
            }

            return showFormat;
        }

        public static string ShowFileSizeText(long BFileSize)
        {
            if (BFileSize < 0)
                return "0B";
            if (BFileSize < 1024)
            {
                return BFileSize + "B";
            }
            else
                return ShowFileSizeText(BFileSize / 1024.00);
        }

        /// <summary>
        /// 返回以M为单位的文件大小
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static double GetFileSizeM(double KBfileSize)
        {
            double m = 0;
            if (KBfileSize < 0)
                return m;
            m = (double)(KBfileSize / 1024.00);
            return m;
        }

        ///// <summary>
        ///// 获取文件的绝对路径,针对window程序和web程序都可使用
        ///// </summary>
        ///// <param name="relativePath">相对路径地址</param>
        ///// <returns>绝对路径地址</returns>
        //public static string GetAbsolutePath(string relativePath)
        //{
        //    if (string.IsNullOrEmpty(relativePath))
        //    {
        //        throw new ArgumentNullException("参数relativePath空异常！");
        //    }
        //    relativePath = relativePath.Replace("/", "\\");
        //    if (relativePath[0] == '\\')
        //    {
        //        relativePath = relativePath.Remove(0, 1);
        //    }
        //    //判断是Web程序还是window程序
        //    if (HttpContext.Current != null)
        //    {
        //        return Path.Combine(HttpRuntime.AppDomainAppPath, relativePath);
        //    }
        //    else
        //    {
        //        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        //    }
        //}

    }

}
