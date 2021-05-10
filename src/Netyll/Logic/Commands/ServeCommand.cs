using Microsoft.AspNetCore.Connections;
using Netyll.Logic.Hosting;
using Netyll.Logic.Templating.Context;
using Netyll.Logic.Templating.Engines;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Diagnostics;
using System.Threading.Tasks;
using Netyll.Logic.Extensions;

namespace Netyll.Logic.Commands
{
    public class ServeCommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISiteEngine _engine;
        private readonly SiteContextGenerator _generator;
        private readonly Modules.IFileSystemWatcher _watcher;

        public ServeCommand(IFileSystem fileSystem, ISiteEngine siteEngine, SiteContextGenerator generator, Modules.IFileSystemWatcher watcher)
        {
            _fileSystem = fileSystem;
            _engine = siteEngine;
            _generator = generator;
            _watcher = watcher;
            SourcePath = _fileSystem.DirectoryInfo.FromDirectoryName(Constants.DEFAULT_SOURCE_PATH);
            DestinationPath = _fileSystem.DirectoryInfo.FromDirectoryName(Constants.DEFAULT_DESTINATION_PATH);
        }

        public IDirectoryInfo SourcePath { get; set; }
        public IDirectoryInfo DestinationPath { get; set; }
        public bool IncludeDrafts { get; set; } = false;
        public bool CleanDestination { get; set; } = true;
        public int Port { get; set; }

        //public IEnumerable<ITransform> Transforms { get; set; }

        public async Task<int> Run()
        {
            var siteContext = _generator.BuildContext(SourcePath, DestinationPath, IncludeDrafts);

            if (CleanDestination && siteContext.OutputFolder.Exists)
                siteContext.OutputFolder.Delete(true);

            _engine.Initialize();
            _engine.Process(siteContext);
            //figure out what this is and how to use it
            //foreach (var t in Transforms)
            //    t.Transform(siteContext);

            _watcher.OnChange(SourcePath, DestinationPath, file => watcherOnChanged(file));

            using (var w = new AspNetCoreWebHost(DestinationPath, Port, debug: true))
            {
                try
                {
                    await w.Start();
                }
                catch(IOException ex) when (ex.InnerException is AddressInUseException)
                {
                    Trace.TraceInformation("Port {0} is already in use", Port);
                    return 1;
                }

                var url = $"http://localhost:{Port}";

                try
                {
                    Trace.TraceInformation("Opening {0} in default browser...", url);
                    var psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch
                {
                    Trace.TraceInformation("Failed to launch {0}", url);
                    Trace.TraceInformation("Browse to {0} to view the site.", url);
                }
            }

            Trace.TraceInformation("Press 'Q' to stop the web host...");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            }
            while (key.Key != ConsoleKey.Q);

            Console.WriteLine();

            return 0;
        }

        private void watcherOnChanged(string file)
        {
            var source = SourcePath.FullName;
            if(file.StartsWith(source))
            {
                var relativeFile = file.Substring(source.Length).ToRelativeFile();
                if (_generator.IsExcludedPath(relativeFile))
                    return;
            }

            Trace.TraceInformation("File change: {0}", file);

            var siteContext = _generator.BuildContext(SourcePath, DestinationPath, IncludeDrafts);

            if (CleanDestination && siteContext.OutputFolder.Exists)
                siteContext.OutputFolder.Delete(true);

            _engine.Process(siteContext);

            //figure out what this is and how to use it
            //foreach (var t in Transforms)
            //    t.Transform(siteContext);
        }
    }
}
