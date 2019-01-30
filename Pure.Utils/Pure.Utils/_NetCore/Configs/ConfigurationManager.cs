using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO; 

namespace Pure.Utils
{
    public class ConfigurationManager
    {
        public static IConfiguration Configuration;

        //static ConfigurationManager()
        //{
        //    Configuration = new ConfigurationBuilder()
        //       .SetBasePath(PathHelper.GetBaseRootPath())
        //       .AddJsonFile("appsettings.json", optional: true)
        //       .Build();
        //}
         
        /// <summary>
        /// 注入配置信息
        /// </summary>
        /// <param name="config"></param>
        public static void RegisterConfiguration(IConfiguration config)
        {
            Configuration = config;
        }


        #region GetJsonConfig(获取Json配置文件)

        /// <summary>
        /// 获取Json配置文件
        /// </summary>
        /// <param name="configFileName">配置文件名。默认：appsettings.json</param>
        /// <param name="basePath">基路径</param>
        /// <returns></returns>
        public static IConfigurationRoot GetJsonConfig(string configFileName = "appsettings.json", string basePath = "")
        {
            basePath = string.IsNullOrWhiteSpace(basePath)
                ? Directory.GetCurrentDirectory()
                : Path.Combine(Directory.GetCurrentDirectory(), basePath);

            var configuration = new ConfigurationBuilder().SetBasePath(basePath)
                .AddJsonFile(configFileName, false, true)
                .Build();

            return configuration;
        }

        #endregion

        #region GetXmlConfig(获取Xml配置文件)

        /// <summary>
        /// 获取Xml配置文件
        /// </summary>
        /// <param name="configFileName">配置文件名。默认：appsettings.xml</param>
        /// <param name="basePath">基路径</param>
        /// <returns></returns>
        public static IConfigurationRoot GetXmlConfig(string configFileName = "appsettings.xml", string basePath = "")
        {
            basePath = string.IsNullOrWhiteSpace(basePath)
                ? Directory.GetCurrentDirectory()
                : Path.Combine(Directory.GetCurrentDirectory(), basePath);

            var configuration = new ConfigurationBuilder().AddXmlFile(config =>
            {
                config.Path = configFileName;
                config.FileProvider = new PhysicalFileProvider(basePath);
            });

            return configuration.Build();
        }

        #endregion

        public static IConfigurationSection GetSection(string key) { return Configuration.GetSection(key); }
        public static string GetValue(string key,  string def="")
        {
            var val = Configuration.GetValue<string>(key);
            if (string.IsNullOrEmpty(val))
            {
                return def;
            }

            return val;
        }
         
        public static T GetSection<T>(  string key = null)
            where T : new()
        {
            if (Configuration == null)
            {
                throw new ArgumentNullException(nameof(Configuration));
            }

            if (key == null)
            {
                key = typeof(T).Name;
            }

            var section = new T();
            Configuration.GetSection(key).Bind(section);
            return section;
        }
        public static T GetValue<T>(string key, T def  ) 
        {
            var val= Configuration.GetValue<T>(key);
            if (val == null)
            {
                return def;
            }
            return val;
        }

        private static Dictionary<string, object> _AppSettings;
        public static Dictionary<string, object> AppSettings
        {
            get
            {
                if (_AppSettings == null  )
                {
                    _AppSettings= GetSection<Dictionary<string,object>>("AppSettings");
                    if (_AppSettings == null)
                    {
                        return new Dictionary<string, object>();
                    }
                }
               
                return _AppSettings;
            }
        }
        private static ConnectionStringSettingsCollection _ConnectionStrings;
        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get
            {
                if (_ConnectionStrings == null)
                {
                    _ConnectionStrings = new ConnectionStringSettingsCollection();

                    List<ConnectionStringSettings> nv = GetSection<List<ConnectionStringSettings>>("ConnectionStrings");
                    if (nv != null && nv.Count > 0)
                    {
                        foreach (var kv in nv)
                        {
                            _ConnectionStrings.Add(kv);
                        }
                    }

                }

                return _ConnectionStrings;
            }
        }

    }
}