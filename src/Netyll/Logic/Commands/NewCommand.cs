using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.IO.Abstractions;
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
        }

        public int Run()
        {
            var includesPath = Path.Combine(Environment.CurrentDirectory, "_includes");
            if (!_fileSystem.Directory.Exists(includesPath))
                _fileSystem.Directory.CreateDirectory(includesPath);

            var layoutsPath = Path.Combine(Environment.CurrentDirectory, "_layouts");
            if (!_fileSystem.Directory.Exists(layoutsPath))
                _fileSystem.Directory.CreateDirectory(layoutsPath);

            var postsPath = Path.Combine(Environment.CurrentDirectory, "_posts");
            if (!_fileSystem.Directory.Exists(postsPath))
                _fileSystem.Directory.CreateDirectory(postsPath);

            var cssPath = Path.Combine(Environment.CurrentDirectory, "css");
            if (!_fileSystem.Directory.Exists(cssPath))
                _fileSystem.Directory.CreateDirectory(cssPath);

            copyResource("Resources/_includes/head.html", Path.Combine(includesPath, "head.html"));
            copyResource("Resources/_layouts/layout.html", Path.Combine(layoutsPath, "layout.html"));
            copyResource("Resources/_layouts/post.html", Path.Combine(layoutsPath, "post.html"));
            copyResource("Resources/_posts/myfirstpost.md", Path.Combine(postsPath, $"{DateTime.Today:yyyy-MM-dd}-myfirstpost.md"));
            copyResource("Resources/css/style.css", Path.Combine(cssPath, "style.css"));
            copyResource("Resources/_config.yml", Path.Combine(Environment.CurrentDirectory, "_config.yml"));
            copyResource("Resources/25.png", Path.Combine(Environment.CurrentDirectory, "25.png"));
            copyResource("Resources/about.md", Path.Combine(Environment.CurrentDirectory, "about.md"));
            copyResource("Resources/atom.xml", Path.Combine(Environment.CurrentDirectory, "atom.xml"));
            copyResource("Resources/favicon.ico", Path.Combine(Environment.CurrentDirectory, "favicon.ico"));
            copyResource("Resources/favicon.png", Path.Combine(Environment.CurrentDirectory, "favicon.png"));
            copyResource("Resources/index.html", Path.Combine(Environment.CurrentDirectory, "index.html"));
            copyResource("Resources/logo.png", Path.Combine(Environment.CurrentDirectory, "logo.png"));
            copyResource("Resources/rss.xml", Path.Combine(Environment.CurrentDirectory, "rss.xml"));
            copyResource("Resources/sitemap.xml", Path.Combine(Environment.CurrentDirectory, "sitemap.xml"));

            return 0;
        }

        private void copyResource(string resourceName, string fileName)
        {
            using (var resourceStream = _embeddedFileProvider.GetFileInfo(resourceName).CreateReadStream())
            using (var fileStream = File.Create(fileName))
            {
                resourceStream.CopyTo(fileStream);
                fileStream.Close();
                resourceStream.Close();
            }
        }
    }
}
