using Netyll.Logic.Extensions;
using Netyll.Logic.Templating.Context;
using Netyll.Logic.Templating.Engines;
using System;
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
        }

        public int Run(bool includeDrafts, bool cleanTarget)
        {
            var siteContext = _generator.BuildContext(Environment.CurrentDirectory, Path.Combine(Environment.CurrentDirectory, "_site"), includeDrafts);

            if (cleanTarget && _fileSystem.Directory.Exists(siteContext.OutputFolder))
                _fileSystem.Directory.Delete(siteContext.OutputFolder);

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
