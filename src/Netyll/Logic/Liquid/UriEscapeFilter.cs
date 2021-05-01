using System;

namespace Netyll.Logic.Liquid
{
    public static class UriEscapeFilter
    {
        public static string uri_escape(string input)
        {
            return Uri.EscapeUriString(input);
        }
    }
}
