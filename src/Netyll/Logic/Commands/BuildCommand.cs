using Netyll.Logic.Extensions;
using Netyll.Logic.Templating.Context;
using Netyll.Logic.Templating.Engines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;

namespace Netyll.Logic.Commands
{
    public class BuildCommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISiteEngine _engine;
        private readonly SiteContextGenerator _generator;

        public BuildCommand(IFileSystem fileSystem, ISiteEngine siteEngine, SiteContextGenerator generator)
        {
            _fileSystem = fileSystem;
            _engine = siteEngine;
            _generator = generator;
            SourcePath = _fileSystem.DirectoryInfo.FromDirectoryName(Constants.DEFAULT_SOURCE_PATH);
            DestinationPath = _fileSystem.DirectoryInfo.FromDirectoryName(Constants.DEFAULT_DESTINATION_PATH);
        }

        public IDirectoryInfo SourcePath { get; set; }
        public IDirectoryInfo DestinationPath { get; set; }
        public bool IncludeDrafts { get; set; } = false;
        public bool CleanDestination { get; set; } = true;
        
        //public IEnumerable<ITransform> Transforms { get; set; }

        public int Run()
        {
            var siteContext = _generator.BuildContext(SourcePath, DestinationPath, IncludeDrafts);

            if (CleanDestination && siteContext.OutputFolder.Exists)
                siteContext.OutputFolder.Delete(true);

            var watch = new Stopwatch();
            watch.Start();
            _engine.Initialize();
            _engine.Process(siteContext);

            //figure out what this is and how to use it
            //foreach (var t in Transforms)
            //    t.Transform(siteContext);

            _engine.CompressSitemap(siteContext, _fileSystem);

            watch.Stop();

            // logging
            // Tracing.Info("done - took {0}ms", watch.ElapsedMilliseconds);

            return 0;
        }
    }
}
