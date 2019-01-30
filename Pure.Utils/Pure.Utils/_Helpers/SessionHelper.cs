//using System;
//using System.ComponentModel;
//using System.Web;

//namespace Pure.Utils
//{
//    /// <summary>
//    /// Session 帮助类
//    /// </summary>
//    public class SessionHelper
//    {
//        private static readonly string SessionUser = "SESSION_USER";
//        public static void AddSessionUser<T>(T user)
//        {
//            HttpContext rq = HttpContext.Current;
//            rq.Session[SessionUser] = user;
//        }
//        public static T GetSessionUser<T>()
//        {
//            try
//            {
//                HttpContext rq = HttpContext.Current;
//                return (T)rq.Session[SessionUser];
//            }
//            catch (Exception e)
//            {
//                throw new Exception(e.Message);
//            }
//        }

//        public static void Clear()
//        {
//            HttpContext rq = HttpContext.Current;
//            rq.Session[SessionUser] = null;
//        }
//    }
//}
