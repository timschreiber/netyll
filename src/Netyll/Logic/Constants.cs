using System;
using System.IO;

namespace Netyll.Logic
{
    public static class Constants
    {
        public static readonly string DEFAULT_SOURCE_PATH = Environment.CurrentDirectory;
        public static readonly string DEFAULT_DESTINATION_PATH = Path.Combine(Environment.CurrentDirectory, "_site");
    }
}
