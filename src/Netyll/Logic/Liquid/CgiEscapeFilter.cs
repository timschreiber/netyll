﻿using System;

namespace Netyll.Logic.Liquid
{
    public static class CgiEscapeFilter
    {
        public static string cgi_escape(string input)
        {
            return Uri.EscapeDataString(input);
        }
    }
}
