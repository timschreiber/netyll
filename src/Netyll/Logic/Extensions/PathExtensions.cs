﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Netyll.Logic.Extensions
{
    public static class PathExtensions
    {
        private static readonly string[] MarkdownFiles = new[] { ".md", ".mkd", ".mkdn", ".mdown", ".markdown" };
        private static readonly string[] ImageFiles = new[] { ".png", ".gif", ".jpg" };

        public static bool IsMarkdownFile(this string extension)
        {
            return MarkdownFiles.Contains(extension.ToLower(CultureInfo.InvariantCulture));
        }

        public static string ToRelativeFile(this string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        }

        public static bool IsImageFormat(this string extension)
        {
            return ImageFiles.Contains(extension.ToLower(CultureInfo.InvariantCulture));
        }
    }
}
