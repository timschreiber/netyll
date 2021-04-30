using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;
using Netyll.Commands;

namespace Netyll
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "Netyll.exe",
                FullName = "Netyll",
                Description = "A simple, Jekyll compatible static site generation tool for .NET Developers and Windows users."
            };

            app.HelpOption("-?|-h|--help");

            app.Command("new", cmd =>
            {
                cmd.Description = "Creates a new blank Jekyll site scaffold at current path.";
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(new NewCommand().Run);
            });

            app.Command("build", cmd =>
            {
                cmd.Description = "Performs a one off build your site to ./_site (by default).";
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(new BuildCommand().Run);
            });

            app.Command("serve", cmd =>
            {
                cmd.Description = "Builds your site any time a source file changes and serves it locally.";
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(new ServeCommand().Run);
            });

            app.Command("clean", cmd =>
            {
                cmd.Description = "Removes all generated files: destination folder, metadata file, Sass and Netyll caches.";
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(new CleanCommand().Run);
            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            app.Execute(args);
        }
    }
}
