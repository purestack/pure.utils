using System;
using System.Collections.Generic;
using System.Collections.Specialized;
namespace Pure.Utils
{
    public class AppConfigHelper : Singleton<AppConfigHelper>
    {

        public Dictionary<string, object> LoadAllAppSettings()
        {
            return ConfigurationManager.AppSettings;
        }
        public T GetWithCache<T>(string key)
        {
            var appSetting = GetByKey(key);
            if (appSetting != null) throw new Exception(String.Format("Could not find setting '{0}',", key));

            //var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(appSetting);
        }

        public T Get<T>(string key)
        {
            var appSetting = GetByKey(key);
            if (appSetting != null) throw new Exception(String.Format("Could not find setting '{0}',", key));

            //var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(appSetting);
        }

        public T Get<T>(string key, T defaultValue)
        {
            var appSetting = GetByKey(key);
            if (appSetting == null || string.IsNullOrEmpty(appSetting.ToString())) return defaultValue;

            try
            {
                //var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)(appSetting);
            }
            catch (Exception)
            {
                return defaultValue;
            }

        }

        public bool TryGet(string key, out string value)
        {
            var appSetting = GetByKey(key);
            if (appSetting == null || (appSetting.ToString() == ""))
            {
                value = "";
                return false;
            }

            value = appSetting.ToString();
            return true;
        }

        public bool ExistsAppConfig(string newKey)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.GetValue(newKey));

            //foreach (var item in ConfigurationManager.AppSettings)
            //{
            //    if (item.Key == newKey)
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }


        private object GetByKey(string newKey)
        {
            return ConfigurationManager.GetValue(newKey);
            //if (ConfigurationManager.AppSettings.ContainsKey(newKey))
            //{
            //    return ConfigurationManager.AppSettings[newKey];
            //}
            //return null;
        }

        ///<summary>
        ///在＊.exe.config文件中appSettings配置节增加一对键、值对
        ///</summary>
        ///<param ></param>
        ///<param ></param>
        public void UpdateAppConfig(string newKey, string newValue, bool overwrite = true)
        {
            //bool isModified = false;
            //foreach (var item in ConfigurationManager.AppSettings)
            //{
            //    if (item.Key == newKey && overwrite)
            //    {
            //        isModified = true;
            //        break;
            //    }
            //}

            //// Open App.Config of executable
            //Configuration config =
            //    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //// You need to remove the old settings object before you can replace it
            //if (isModified)
            //{
            //    config.AppSettings.Settings.Remove(newKey);
            //}
            //// Add an Application Setting.
            //config.AppSettings.Settings.Add(newKey, newValue);
            //// Save the changes in App.config file.
            //config.Save(ConfigurationSaveMode.Modified);
            //// Force a reload of a changed section.
            //ConfigurationManager.RefreshSection("appSettings");
        }

        ///<summary>
        ///依据连接串名字connectionName返回数据连接字符串
        ///</summary>
        ///<param ></param>
        ///<returns></returns>
        public string GetConnectionStringsConfig(string connectionName)
        {
            string connectionString =
                    ConfigurationManager.ConnectionStrings[connectionName].ConnectionString.ToString();
            Console.WriteLine(connectionString);
            return connectionString;
        }

        ///<summary>
        ///更新连接字符串
        ///</summary>
        ///<param >连接字符串名称</param>
        ///<param >连接字符串内容</param>
        ///<param >数据提供程序名称</param>
        public void UpdateConnectionStringsConfig(string newName, string newConString, string newProviderName)
        {
            //bool isModified = false;    //记录该连接串是否已经存在
            ////如果要更改的连接串已经存在
            //if (ConfigurationManager.ConnectionStrings[newName] != null)
            //{
            //    isModified = true;
            //}
            ////新建一个连接字符串实例
            //ConnectionStringSettings mySettings =
            //    new ConnectionStringSettings(newName, newConString, newProviderName);
            //// 打开可执行的配置文件*.exe.config
            //Configuration config =
            //    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //// 如果连接串已存在，首先删除它
            //if (isModified)
            //{
            //    config.ConnectionStrings.ConnectionStrings.Remove(newName);
            //}
            //// 将新的连接串添加到配置文件中.
            //config.ConnectionStrings.ConnectionStrings.Add(mySettings);
            //// 保存对配置文件所作的更改
            //config.Save(ConfigurationSaveMode.Modified);
            //// 强制重新载入配置文件的ConnectionStrings配置节
            //ConfigurationManager.RefreshSection("ConnectionStrings");
        }
        //public Configuration LoadConfig(string configPath)
        //{
        //    Configuration config = ConfigurationManager.OpenExeConfiguration(configPath);
        //    return config;
        //}


        public NameValueCollection GetSectionNameValueCollection(string sectionName)
        {
            var v = ConfigurationManager.GetSection(sectionName) as NameValueCollection;
            return v;
        }

        public bool ExistsInSection(string sectionName, string newKey)
        {
            var v = GetSectionNameValueCollection(sectionName);
            if (v != null)
            {
                return v[newKey] != null;
            }
            return false;
        }
        public object GetValueInSection(string sectionName, string key)
        {
            var v = GetSectionNameValueCollection(sectionName);
            if (v != null)
            {
                return v[key];
            }
            return null;
        }

        public void SetValueInSection(string sectionName, string newKey, string newValue, bool overwrite = true)
        {
            //var v = GetSectionNameValueCollection(sectionName);
            //if (v != null)
            //{


            //    bool isModified = false;
            //    foreach (string key in v.Keys)
            //    {
            //        if (key == newKey && overwrite)
            //        {
            //            isModified = true;
            //        }
            //    }

            //    // Open App.Config of executable
            //    Configuration config =
            //        ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //    // You need to remove the old settings object before you can replace it
            //    if (isModified)
            //    {
            //        v.Remove(newKey);
            //    }
            //    v.Add(newKey, newValue);
            //    // Add an Application Setting.
            //    //config.AppSettings.Settings.Add(newKey, newValue);
            //    // Save the changes in App.config file.
            //    config.Save(ConfigurationSaveMode.Modified);
            //    // Force a reload of a changed section.
            //    ConfigurationManager.RefreshSection(sectionName);
            //}

        }


    }
}
