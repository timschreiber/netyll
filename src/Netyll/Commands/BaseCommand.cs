using System;
using System.IO;

namespace Netyll.Commands
{
    public abstract class BaseCommand
    {
        protected DirectoryInfo CurrentDirectory => new DirectoryInfo(Environment.CurrentDirectory);

        public abstract int Run();
    }
}
