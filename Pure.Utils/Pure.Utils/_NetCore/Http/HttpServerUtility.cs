namespace Pure.Utils
{
    public class HttpServerUtility
    {
        public string HtmlDecode(string s)
        {
            return System.Net.WebUtility.HtmlDecode(s);
        }
        public string HtmlEncode(string s)
        {
            return System.Net.WebUtility.HtmlDecode(s);
        }
        public string UrlEncode(string s)
        {
            return System.Net.WebUtility.UrlEncode(s);
        }
        public string UrlDecode(string s)
        {
            return System.Net.WebUtility.UrlDecode(s);
        }
        public string MapPath(string path)
        {

             
            //path = path.Replace("~/", "").Trim('/').Trim('\\');
            //return System.IO.Path.Combine(GlobalHostEnvironment.WebRootPath, path.ToFilePath());

           // return GlobalHostEnvironment.ContentRootPath.TrimEnd('/') + "/" + path.TrimStart('~', '/').Replace("/", "\\");

            return (GlobalHostEnvironment.ContentRootPath.TrimEnd('/') + "/" + path.TrimStart('~', '/')).Replace("/", "\\");
        }
    }
}