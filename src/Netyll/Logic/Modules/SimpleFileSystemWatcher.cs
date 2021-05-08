using System;
using System.IO;
using System.IO.Abstractions;

namespace Netyll.Logic.Modules
{
    public class SimpleFileSystemWatcher : IFileSystemWatcher, IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private string _destinationPath;
        private string _lastFile;
        private Action<string> _callback;

        public SimpleFileSystemWatcher()
        {
            _watcher = new FileSystemWatcher();
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= watcherOnChanged;
            _watcher.Created -= watcherOnChanged;
        }

        public void OnChange(IDirectoryInfo sourcePath, IDirectoryInfo destinationPath, Action<string> fileChangedCallback)
        {
            _destinationPath = destinationPath.FullName;
            _watcher.Path = sourcePath.FullName;
            _watcher.Filter = "*.*";
            _watcher.IncludeSubdirectories = true;
            _watcher.Changed += watcherOnChanged;
            _watcher.Created += watcherOnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        private void watcherOnChanged(object sender, FileSystemEventArgs args)
        {
            if (args.FullPath.Contains(_destinationPath))
                return;

            if(args.FullPath == _lastFile)
            {
                _lastFile = "";
                return;
            }

            _callback(args.FullPath);

            _lastFile = args.FullPath;
        }
    }
}
