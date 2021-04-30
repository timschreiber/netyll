using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Netyll.Commands
{
    public class NewCommand : BaseCommand
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        public NewCommand()
        {
            _embeddedFileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        }

        public override int Run()
        {
            copyResource("Scaffolds/blank/_config.yml", $"{CurrentDirectory.FullName}\\_config.yml");
            copyResource("Scaffolds/blank/index.html", $"{CurrentDirectory.FullName}\\index.html");
            var layoutsDirectory = CurrentDirectory.CreateSubdirectory("_layouts");
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
