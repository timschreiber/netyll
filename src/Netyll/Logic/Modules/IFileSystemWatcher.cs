using System;
using System.IO.Abstractions;

namespace Netyll.Logic.Modules
{
    public interface IFileSystemWatcher
    {
        void OnChange(IDirectoryInfo sourcePath, IDirectoryInfo destinationPath, Action<string> fileChangedCallback);
    }
}
