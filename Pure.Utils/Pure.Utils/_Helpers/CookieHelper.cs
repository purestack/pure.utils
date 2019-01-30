using System;
using Microsoft.AspNetCore.Http;

namespace Pure.Utils
{
    /// <summary>
    /// Cookie帮助类
    /// </summary>
    public class CookieHelper
    {
        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        public static void WriteCookie(Microsoft.AspNetCore.Http.HttpContext context, string strName, string strValue)
        {

#if !NET45

            context.Response.Cookies.Append(strName, strValue);
#else
         HttpCookie cookie =context.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            context.Response.AppendCookie(cookie);
#endif



        }
        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        /// <param name="strValue">过期时间(分钟)</param>
        public static void WriteCookie(Microsoft.AspNetCore.Http.HttpContext context, string strName, string strValue, int expires, string domain = "", string path = "/", bool httponly = true, bool secure = false)
        {



#if !NET45

            context.Response.Cookies.Append(strName, strValue, new CookieOptions()
            {
                Domain = domain,
                Path = path,
                HttpOnly = httponly,
                Secure = secure,
                Expires = new DateTimeOffset(DateTime.Now.AddMinutes(expires))
            });
#else
            
            HttpCookie cookie = context.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now.AddMinutes(expires);

            cookie.HttpOnly = httponly;
            cookie.Secure = secure;
            cookie.Path = path;
           
            if (domain != null)
                cookie.Domain = domain;

            context.Response.AppendCookie(cookie);
#endif


        }

        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <returns>cookie值</returns>
        public static string GetCookie(Microsoft.AspNetCore.Http.HttpContext context, string strName)
        {



#if !NET45
            return context.Request.Cookies[strName];

#else
if (context.Request.Cookies[strName] != null)
            {
                return context.Request.Cookies[strName].Value;
            }
            return "";
#endif


        }


        /// <summary>
        /// Set cookie value using the token and the expiry date
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static string BuildCookueValue(string value, int minutes)
        {
            return String.Format("{0}|{1}", value, DateTime.Now.AddMinutes(minutes).ToString());
        }



#if !NET45

#else

                /// <summary>
        /// Get cookie expiry date that was set in the cookie value 
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static DateTime GetExpirationDate(HttpCookie cookie)
        {
            if (String.IsNullOrEmpty(cookie.Value))
            {
                return DateTime.MinValue;
            }
            string strDateTime = cookie.Value.Substring(cookie.Value.IndexOf("|") + 1);
            return Convert.ToDateTime(strDateTime);
        }


   /// <summary>
        /// Reads cookie value from the cookie
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static string GetCookieValue(HttpCookie cookie)
        {
            if (String.IsNullOrEmpty(cookie.Value))
            {
                return cookie.Value;
            }
            return cookie.Value.Substring(0, cookie.Value.IndexOf("|"));
        }
#endif


        /// <summary>
        /// 清除指定Cookie
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        public static void ClearCookie(Microsoft.AspNetCore.Http.HttpContext context, string cookiename)
        {




#if !NET45

            context.Response.Cookies.Delete(cookiename);

#else

            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            if (cookie != null)
            {
                cookie.Name = cookiename;
                cookie.Expires = DateTime.Today.AddDays(-1);
                if (HttpContext.Current.Response.Cookies[cookiename] != null)
                {
                    HttpContext.Current.Response.Cookies[cookiename].Expires = DateTime.Now.AddDays(-1);

                }
                HttpContext.Current.Response.Cookies.Add(cookie);
                
                HttpContext.Current.Request.Cookies.Remove(cookiename);
                 
            }
#endif

        }

    }
}
