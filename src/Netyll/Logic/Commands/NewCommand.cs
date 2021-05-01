using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Reflection;

namespace Netyll.Logic.Commands
{
    public class NewCommand
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        public NewCommand()
        {
            _embeddedFileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        }

        public int Run()
        {
            copyResource("Scaffolds/blank/_config.yml", $"{Environment.CurrentDirectory}\\_config.yml");
            copyResource("Scaffolds/blank/index.html", $"{Environment.CurrentDirectory}\\index.html");
            var layoutsDirectory = new DirectoryInfo(Environment.CurrentDirectory).CreateSubdirectory("_layouts");
            copyResource("Scaffolds/blank/_layouts/default.html", $"{layoutsDirectory.FullName}\\default.html");
            Console.WriteLine($"{nameof(NewCommand)}.{nameof(Run)}");
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
