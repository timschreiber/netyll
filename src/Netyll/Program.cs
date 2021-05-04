using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Netyll.Logic.Commands;
using Netyll.Logic.Extensibility;
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
            services.AddTransient<ILightweightMarkupEngine, MarkdigMarkdownEngine>();
            services.AddTransient<ISiteEngine, LiquidEngine>();
            services.AddTransient<SiteContextGenerator>();
            services.AddTransient<LinkHelper>();
            services.AddTransient<Logic.IConfiguration, Logic.Configuration>();
            _serviceProvider = services.BuildServiceProvider();
        }

        public static void Main(string[] args)
        {
            var _fileSystem = _serviceProvider.GetService<IFileSystem>();

            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "Netyll.exe",
                FullName = "Netyll",
                Description = "A simple, Jekyll-compatible static site generation tool for .NET Developers and Windows users."
            };

            app.HelpOption("-?|-h|--help");

            app.Command("new", cmd =>
            {
                cmd.Description = "Creates a new blank Netyll site template.";
                var pathOption = cmd.Option("--path", "The path where the site template will be created (Default: %CD%).", CommandOptionType.SingleValue);
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(() =>
                {
                    var newCommand = _serviceProvider.GetService<NewCommand>();
                    newCommand.BasePath = _fileSystem.DirectoryInfo.FromDirectoryName(pathOption.HasValue()
                        ? pathOption.Value()
                        : Environment.CurrentDirectory);

                    return newCommand.Run();
                });
            });

            app.Command("build", cmd =>
            {
                cmd.Description = "Performs a one off build your site to ./_site (by default).";
                var sourcePathOption = cmd.Option("--source-path", "The path where the source site template is located (Default: %CD%).", CommandOptionType.SingleValue);
                var destinationPathOption = cmd.Option("--destination-path", "The path where the generated site will be written (Default: %CD%\\_site).", CommandOptionType.SingleValue);
                var includeDraftsOption = cmd.Option("--include-drafts", "Include draft pages and posts when building the site.", CommandOptionType.NoValue);
                var cleanTargetOption = cmd.Option("--clean-target", "Clean the destination directory before building the site.", CommandOptionType.NoValue);
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(() =>
                {
                    var buildCommand = _serviceProvider.GetService<BuildCommand>();

                    var sourcePath = sourcePathOption.HasValue()
                        ? sourcePathOption.Value()
                        : Environment.CurrentDirectory;

                    buildCommand.SourcePath = _fileSystem.DirectoryInfo.FromDirectoryName(sourcePath);

                    buildCommand.DestinationPath = _fileSystem.DirectoryInfo.FromDirectoryName(destinationPathOption.HasValue()
                        ? destinationPathOption.Value()
                        : Path.Combine(sourcePath, "_site"));

                    buildCommand.IncludeDrafts  = includeDraftsOption != null && includeDraftsOption.HasValue();
                    buildCommand.CleanDestination = cleanTargetOption != null && cleanTargetOption.HasValue();

                    return buildCommand.Run();
                });
            });

            app.Command("serve", cmd =>
            {
                cmd.Description = "Builds your site any time a source file changes and serves it locally.";
                var sourcePathOption = cmd.Option("--source-path", "The path where the source site template is located (Default: %CD%).", CommandOptionType.SingleValue);
                var destinationPathOption = cmd.Option("--destination-path", "The path where the generated site will be written (Default: %CD%\\_site).", CommandOptionType.SingleValue);
                var includeDraftsOption = cmd.Option("--include-drafts", "Include draft pages and posts when building the site.", CommandOptionType.NoValue);
                var cleanTargetOption = cmd.Option("--clean-target", "Clean the destination directory before building the site.", CommandOptionType.NoValue);
                cmd.HelpOption("-?|-h|--help");
                cmd.OnExecute(_serviceProvider.GetService<ServeCommand>().Run);
            });

            app.Command("clean", cmd =>
            {
                cmd.Description = "Removes all generated files: destination folder, metadata file, Sass and Netyll caches.";
                var destinationPathOption = cmd.Option("--destination-path", "The path where the generated site will be written (Default: %CD%\\_site)", CommandOptionType.SingleValue);
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
