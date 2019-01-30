using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Utils
{
    public class Toolset
    {
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
