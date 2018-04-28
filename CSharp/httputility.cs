using System.Net;
namespace ByondLang.Override{
    class HttpUtility{
        public static string UrlEncode(string enc){
            return WebUtility.UrlEncode(enc);
        }

        public static string UrlDecode(string enc){
            return WebUtility.UrlDecode(enc);
        }

        public static string HtmlEncode(string enc){
            return WebUtility.HtmlEncode(enc);
        }
    }
}