using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Pure.Utils
{
    public interface IStorageService
    {
        string CreateFolder(string folder);
        string DeleteFolder(string folder);
        string SaveFile(string file);
        string DeleteFile(string file);
    }

    public static class HttpRequestFileExtentions
    {

        /// <summary>
        /// 判断是否为图片
        /// </summary>
        /// <param name="ext">扩展名</param>
        /// <returns>返回Bool值，是则返回true</returns>
        public static bool IsImage(string ext)
        {
            ext = ext.ToLower();
            if (ext == ".gif" || ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".bmp")
            {
                return true;
            }
            else return false;
        }
        public static string ToFilePath( string path)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }
        /// <summary>
        /// 判断文件是否能上传
        /// </summary>
        /// <param name="ext">文件扩展名</param>
        /// <returns></returns>
        public static bool FileCanUp(string ext)
        {
            ext = ext.ToLower();
            var exts = new List<string> { ".aspx", ".asp", ".exe", ".php", ".jsp", ".htm", ".html", ".xhtml", ".cs", ".bat", ".jar", ".dll", ".com" };
            return !exts.Contains(ext);
        }

        #region SaveFile

        public static string UploadFolder = "uploads";
        public static string ImageFolder = "images";
        public static string FileFolder = "files";

        public static string ChangeToWebPath(this HttpRequest request, string path)
        {
            return path.Replace(request.MapPath("~/"), "~").Replace("\\", "/");
        }
        public static string GetUploadPath(this HttpRequest request, string folder = "Images")
        {
            
            var path = Path.Combine(new string[] { GlobalHostEnvironment.WebRootPath, UploadFolder, folder, DateTime.Now.ToString("yyyyMM") });
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                var storage = request.HttpContext.RequestServices.GetService<IStorageService>();
                if (storage != null)
                {
                    storage.CreateFolder(path);
                }
            }
            return path;
        }
        public static string MapPath(this HttpRequest request, string path)
        { 
            path = path.Replace("~/", "").Trim('/').Trim('\\');
            return Path.Combine(GlobalHostEnvironment.WebRootPath, ToFilePath(path));
        }
        /// <summary>
        /// 保存图片到UpLoad/Images
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string SaveImage(this HttpRequest request)
        {
            if (request.Form.Files.Count > 0 && request.Form.Files[0].Length > 0)
            {
                string path = request.GetUploadPath(ImageFolder);
                string fileName = request.Form.Files[0].FileName;
                string ext = Path.GetExtension(fileName);
                if (IsImage(ext))
                {
                    path = Path.Combine(path, string.Format("{0}{1}", Guid.NewGuid().ToString("N"), ext));
                    request.Form.Files[0].SaveAs(path);
                    var storage = request.HttpContext.RequestServices.GetService<IStorageService>();
                    if (storage != null)
                    {
                        string filePath = storage.SaveFile(path);
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            return filePath;
                        }
                    }
                    return request.ChangeToWebPath(path);
                }
            }
            return string.Empty;
        }
        public static string SaveImage(this HttpRequest request, string name)
        {
            if (request.Form.Files.Count > 0 && request.Form.Files[name].Length > 0)
            {
                string path = request.GetUploadPath(ImageFolder);
                string fileName = request.Form.Files[name].FileName;
                string ext = Path.GetExtension(fileName);
                if (IsImage(ext))
                {
                    fileName = string.Format("{0}{1}", Guid.NewGuid().ToString("N"), ext);
                    path = Path.Combine(path, fileName);
                    request.Form.Files[name].SaveAs(path);
                    var storage = request.HttpContext.RequestServices.GetService<IStorageService>();
                    if (storage != null)
                    {
                        string filePath = storage.SaveFile(path);
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            return filePath;
                        }
                    }
                    return request.ChangeToWebPath(path);
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 保存文件到UpLoad/Files
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string SaveFile(this HttpRequest request)
        {
            if (request.Form.Files.Count > 0 && request.Form.Files[0].Length > 0)
            {
                string path = request.GetUploadPath(FileFolder);
                string fileName = request.Form.Files[0].FileName;
                string ext = Path.GetExtension(fileName);
                if (FileCanUp(ext))
                {
                    fileName = string.Format("{0}{1}", Guid.NewGuid().ToString("N"), ext);
                    path = Path.Combine(path, fileName);
                    request.Form.Files[0].SaveAs(path);
                    var storage = request.HttpContext.RequestServices.GetService<IStorageService>();
                    if (storage != null)
                    {
                        string filePath = storage.SaveFile(path);
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            return filePath;
                        }
                    }
                    return request.ChangeToWebPath(path);
                }
            }
            return string.Empty;
        }
        public static string SaveFile(this HttpRequest request, string name)
        {
            if (request.Form.Files.Count > 0 && request.Form.Files[0].Length > 0)
            {
                string path = request.GetUploadPath(FileFolder);
                string fileName = request.Form.Files[0].FileName;
                string ext = Path.GetExtension(fileName);
                if (FileCanUp(ext))
                {
                    fileName = string.Format("{0}{1}", Guid.NewGuid().ToString("N"), ext);
                    path = Path.Combine(path, fileName);
                    request.Form.Files[0].SaveAs(path);
                    var storage = request.HttpContext.RequestServices.GetService<IStorageService>();
                    if (storage != null)
                    {
                        string filePath = storage.SaveFile(path);
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            return filePath;
                        }
                    }
                    return request.ChangeToWebPath(path);
                }
            }
            return string.Empty;
        }

        public static void DeleteFile(this HttpRequest request, string filePath)
        {
            string file = request.MapPath(filePath);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            var storage = request.HttpContext.RequestServices.GetService<IStorageService>();
            if (storage != null)
            {
                storage.DeleteFile(file);
            }
        }
 
        #endregion



    }
}
