using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Utils
{
    public class Toolset
    {
        public static AppConfigHelper AppConfigHelper
        {
            get { return AppConfigHelper.Instance; }
        }
        /// <summary>
        /// 内部日志记录器（仅用于记录一些配置加载信息和非分布式的日志）,记录日志功能相对简单
        /// </summary>
        public static TinyLogger TinyLogger
        {
            get { return TinyLogger.Instance; }
        }
        /// <summary>
        /// 特殊id生成工具
        /// </summary>
        public static IdGenerateManager IdGenerateManager
        {
            get { return IdGenerateManager.Instance; }
        }
        /// <summary>
        /// 宿主环境
        /// </summary>
        public static IHostingEnvironment HostingEnvironment
        {
            get
            {
                return GlobalHostEnvironment.Current;
            }
        }

        /// <summary>
        /// Http上下文
        /// </summary>
        public static Microsoft.AspNetCore.Http.HttpContext HttpContext
        {
            get
            {
                return Pure.Utils.HttpContext.Current;
            }
        }

        public static HttpContext HttpContextWrappers
        {
            get
            {
                return Pure.Utils.HttpContext.HttpContextWrappers;
            }
        }


    }
}
