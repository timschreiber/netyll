using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Netyll.Logic.Commands
{
    public class NewCommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        public NewCommand(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _embeddedFileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            BasePath = _fileSystem.DirectoryInfo.FromDirectoryName(Environment.CurrentDirectory);
        }

        public IDirectoryInfo BasePath { get; set; }

        public int Run()
        {
            createDirectoryIfNotExists(BasePath);
            createDirectoryIfNotExists(BasePath, "_includes");
            createDirectoryIfNotExists(BasePath, "_layouts");
            createDirectoryIfNotExists(BasePath, "_posts");
            createDirectoryIfNotExists(BasePath, "_data");
            createDirectoryIfNotExists(BasePath, "css");

            copyResource("Resources/_includes/head.html", BasePath);
            copyResource("Resources/_layouts/layout.html", BasePath);
            copyResource("Resources/_layouts/post.html", BasePath);
            copyResource("Resources/_posts/myfirstpost.md", BasePath, $"{DateTime.Today:yyyy-MM-dd}-myfirstpost.md");
            copyResource("Resources/css/style.css", BasePath);
            copyResource("Resources/_config.yml", BasePath);
            copyResource("Resources/25.png", BasePath);
            copyResource("Resources/about.md", BasePath);
            copyResource("Resources/atom.xml", BasePath);
            copyResource("Resources/favicon.ico", BasePath);
            copyResource("Resources/favicon.png", BasePath);
            copyResource("Resources/index.html", BasePath);
            copyResource("Resources/logo.png", BasePath);
            copyResource("Resources/rss.xml", BasePath);
            copyResource("Resources/sitemap.xml", BasePath);

            return 0;
        }

        private void createDirectoryIfNotExists(IDirectoryInfo basePath, string directoryName = null)
        {
            var path = string.IsNullOrWhiteSpace(directoryName)
                ? basePath.FullName
                : Path.Combine(basePath.FullName, directoryName);

            if (!_fileSystem.Directory.Exists(path))
                _fileSystem.Directory.CreateDirectory(path);
        }

        private void copyResource(string resourceName, IDirectoryInfo basePath, string overrideFilename = null)
        {
            var pathParts = resourceName.Split("/").Skip(1).ToList();
            
            if (!string.IsNullOrWhiteSpace(overrideFilename))
            {
                pathParts.Remove(pathParts.Last());
                pathParts.Add(overrideFilename);
            }
            
            pathParts.Insert(0, basePath.FullName);

            var path = Path.Combine(pathParts.ToArray());

            using var resourceStream = _embeddedFileProvider.GetFileInfo(resourceName).CreateReadStream();
            using var fileStream = File.Create(path);
            resourceStream.CopyTo(fileStream);
            fileStream.Close();
            resourceStream.Close();
        }
    }
}
