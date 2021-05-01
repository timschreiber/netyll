using System;
using System.IO.Abstractions;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Netyll.Logic.Commands;
using Netyll.Logic.Templating.Context;
using Netyll.Logic.Templating.Engines;

namespace Netyll
{
    public class Program
    {
        static IServiceProvider _serviceProvider;

        static Program()
        {
            var services = new ServiceCollection();
            services.AddTransient<NewCommand>();
            services.AddTransient<BuildCommand>();
            services.AddTransient<ServeCommand>();
            services.AddTransient<CleanCommand>();
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddTransient<ISiteEngine, LiquidEngine>();
            services.AddTransient<SiteContextGenerator>();
            services.AddTransient<LinkHelper>();
            services.AddTransient<Logic.IConfiguration, Logic.Configuration>();
            _serviceProvider = services.BuildServiceProvider();
        }

        public static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "Netyll.exe",
                FullName = "Netyll",
                Description = "A simple, Jekyll-compatible static site generation tool for .NET Developers and Windows users."
            };

            app.HelpOption("-?|-h|--help");

            app.Command("new", cmd =>
            {
                cmd.Description = "Creates a new blank Netyll site scaffold at current path.";
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(_serviceProvider.GetService<NewCommand>().Run);
            });

            app.Command("build", cmd =>
            {
                cmd.Description = "Performs a one off build your site to ./_site (by default).";
                var includeDraftsOption = cmd.Option("--include-drafts", "Include draft pages and posts when building the site.", CommandOptionType.NoValue);
                var cleanTargetOption = cmd.Option("--clean-target", "Clean the destination directory before building the site.", CommandOptionType.NoValue);
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(() =>
                {
                    var includeDrafts = includeDraftsOption != null && includeDraftsOption.HasValue();
                    var cleanTarget = cleanTargetOption != null && cleanTargetOption.HasValue();
                    return _serviceProvider.GetService<BuildCommand>().Run(includeDrafts, cleanTarget);
                });
            });

            app.Command("serve", cmd =>
            {
                cmd.Description = "Builds your site any time a source file changes and serves it locally.";
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(_serviceProvider.GetService<ServeCommand>().Run);
            });

            app.Command("clean", cmd =>
            {
                cmd.Description = "Removes all generated files: destination folder, metadata file, Sass and Netyll caches.";
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(_serviceProvider.GetService<CleanCommand>().Run);
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
