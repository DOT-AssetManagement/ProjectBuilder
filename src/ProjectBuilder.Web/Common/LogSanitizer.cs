using System.Net;
using Veracode.Attributes;

namespace ProjectBuilder.Web.Common
{
    public static class LogSanitizer
    {
        [CRLFCleanser]
        public static string SanitizeForLog(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // URL-encode so CR/LF and every other control character is neutralized
            return WebUtility.UrlEncode(value);
        }
    }
}
