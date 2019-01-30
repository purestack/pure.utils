using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Pure.Utils
{
    public class GlobalHostEnvironment
    { 
        private static IHostingEnvironment _env;

        public static  IHostingEnvironment Current => _env;
        internal static void Configure(IHostingEnvironment env)
        {
            _env = env;
        }

        public static string WebRootPath { get { return Current.WebRootPath; } }
        public static string ContentRootPath { get { return Current.ContentRootPath; } }
        public static string EnvironmentName { get { return Current.EnvironmentName; } }
        public static string ApplicationName { get { return Current.ApplicationName; } }

        /// <summary>
        /// 转换为绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToAbsolute(string path)
        {
            return path.Replace("~/", "/");
            //return VirtualPathUtility.ToAbsolute(relativeUrl);

        }

        /// <summary>
        /// 获取物理路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <returns></returns>
        public static string GetPhysicalPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            var rootPath =  WebRootPath;
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                return Path.GetFullPath(relativePath);
            }

            return $"{rootPath}\\{relativePath.Replace("/", "\\").TrimStart('\\')}";
        }
        public static string MapPath(string path)
        {


            //path = path.Replace("~/", "").Trim('/').Trim('\\');
            //return System.IO.Path.Combine(GlobalHostEnvironment.WebRootPath, path.ToFilePath());

            // return GlobalHostEnvironment.ContentRootPath.TrimEnd('/') + "/" + path.TrimStart('~', '/').Replace("/", "\\");

            return (　ContentRootPath.TrimEnd('/') + "/" + path.TrimStart('~', '/')).Replace("/", "\\");
        }
    }

}