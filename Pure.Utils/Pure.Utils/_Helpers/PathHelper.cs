
using System;
using System.IO;
using System.Text;

namespace Pure.Utils
{

    public static class PathHelper
    {

        public static string BaseDirectory { get; set; }
        static PathHelper()
        {
#if NET45
              var baseDirectory = AppDomain.CurrentDomain.BaseDirectory; //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;

            //if (AppDomain.CurrentDomain.SetupInformation.PrivateBinPath == null)
            //    baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            BaseDirectory =baseDirectory;
#else
            BaseDirectory = Directory.GetCurrentDirectory();// AppContext.BaseDirectory;// Directory.GetCurrentDirectory();//

#endif
        }
        public static string CombineWithBaseDirectory(string filePath)
        {


            bool isAbsolute = filePath.IndexOf(":") > 0;
            if (!isAbsolute)
            {
                filePath = Path.Combine(PathHelper.GetBaseDirectory(), filePath);
            }
            return filePath;


        }
        /// <summary>
        /// 获取程序根目录
        /// </summary>
        /// <returns></returns>
        public static string GetBaseDirectory()
        {
            return BaseDirectory;
        }
        /// <summary>
        /// 获取dll存放路径
        /// </summary>
        /// <returns></returns>
        public static string GetAppExecuteDirectory()
        {
#if NET45
            var path =  AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            if (path == null)
            path = AppDomain.CurrentDomain.BaseDirectory;
            return path;

#else
            return AppContext.BaseDirectory;
#endif

        }
        public static string GetRootedPath(string path)
        {
            string rootedPath = path ?? string.Empty;
            if (!Path.IsPathRooted(rootedPath))
            {
                if (string.IsNullOrEmpty(BaseDirectory))
                    throw new ArgumentNullException("请先设置BaseDirectory属性");
                rootedPath = Path.Combine(BaseDirectory, rootedPath);
            }
            string directory = Path.GetDirectoryName(rootedPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return rootedPath;
        }

        public static string CombinePath(params string[] paths)
        {
            if (paths.Length == 0)
            {
                throw new ArgumentException("please input path");
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                string spliter = "\\";
                string firstPath = paths[0];
                if (firstPath != null && firstPath.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                {
                    spliter = "/";
                }
                if (firstPath != null && !firstPath.EndsWith(spliter))
                {
                    firstPath = firstPath + spliter;
                }
                builder.Append(firstPath);
                for (int i = 1; i < paths.Length; i++)
                {
                    string nextPath = paths[i];
                    if (nextPath.StartsWith("/") || nextPath.StartsWith("\\"))
                    {
                        nextPath = nextPath.Substring(1);
                    }
                    if (i != paths.Length - 1)//not the last one
                    {
                        if (nextPath.EndsWith("/") || nextPath.EndsWith("\\"))
                        {
                            nextPath = nextPath.Substring(0, nextPath.Length - 1) + spliter;
                        }
                        else
                        {
                            nextPath = nextPath + spliter;
                        }
                    }
                    builder.Append(nextPath);
                }
                string str = builder.ToString();

                return str;
            }
        }

        private const string DATA_DIRECTORY = "|DataDirectory|";

        public static string ExpandPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                return path;

            if (!path.StartsWith(DATA_DIRECTORY, StringComparison.OrdinalIgnoreCase))
                return Path.GetFullPath(path);

            string dataDirectory = GetDataDirectory();
            int length = DATA_DIRECTORY.Length;

            if (path.Length <= length)
                return dataDirectory;

            string relativePath = path.Substring(length);
            char c = relativePath[0];

            if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                relativePath = relativePath.Substring(1);

            string fullPath = Path.Combine(dataDirectory, relativePath);
            fullPath = Path.GetFullPath(fullPath);

            return fullPath;
        }

        public static string GetDataDirectory()
        {
            string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (String.IsNullOrEmpty(dataDirectory))
                dataDirectory = AppDomain.CurrentDomain.BaseDirectory;

            return Path.GetFullPath(dataDirectory);
        }





    }
}
