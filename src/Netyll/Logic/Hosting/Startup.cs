using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace Netyll.Logic.Hosting
{
    public class Startup
    {
        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        { }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var basePath = Configuration.GetValue<string>("basePath");
            //var basePath = "c:\\temp\\netyll\\_site";

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new PhysicalFileProvider(basePath),
                DefaultFileNames = new List<string> { "index.html" }
            });

            var extProvider = new FileExtensionContentTypeProvider();
            extProvider.Mappings.Add(".dll", "application/octet-stream");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(basePath),
                RequestPath = new PathString("")
            });
        }
    }
}
