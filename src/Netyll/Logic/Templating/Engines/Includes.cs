﻿using System.IO;
using System.IO.Abstractions;

namespace Netyll.Logic.Templating.Engines
{
    public class Includes : DotLiquid.FileSystems.IFileSystem
    {
        private readonly IFileSystem _fileSystem;

        public string Root { get; set; }

        public Includes(IDirectoryInfo root, IFileSystem fileSystem)
        {
            Root = root.FullName;
            _fileSystem = fileSystem;
        }

        public string ReadTemplateFile(DotLiquid.Context context, string templateName)
        {
            var include = Path.Combine(Root, "_includes", templateName);
            if (_fileSystem.File.Exists(include))
                return _fileSystem.File.ReadAllText(include);
            return string.Empty;
        }
    }
}
