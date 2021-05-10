using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Netyll.Logic.Hosting
{
    public class AspNetCoreWebHost : IDisposable
    {
        private IHost _host;

        public AspNetCoreWebHost(IDirectoryInfo basePath)
            : this(basePath, 8080, true)
        { }

        public AspNetCoreWebHost(IDirectoryInfo basePath, int port, bool debug)
        {
            IsRunning = false;
            BasePath = basePath == null ? Environment.CurrentDirectory : basePath.FullName;
            Port = port;
            Debug = debug;
        }

        public bool IsRunning { get; private set; }
        public string BasePath { get; }
        public int Port { get; }
        public bool Debug { get; }

        public async Task<bool> Start()
        {
            if (IsRunning)
                return false;

            using var configStream = new MemoryStream();
            using var writer = new StreamWriter(configStream);
            writer.Write(buildJsonConfig());
            writer.Flush();
            configStream.Position = 0;

            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var config = new ConfigurationBuilder()
                        .AddJsonStream(configStream)
                        .Build();

                    webBuilder
                        .UseEnvironment(Debug ? "Debug" : "Production")
                        .ConfigureLogging(logging =>
                        {
                            if (Debug)
                            {
                                logging.AddConsole();
                                logging.AddDebug();
                            }
                        })
                        .UseWebRoot(BasePath)
                        .UseStartup<Startup>()
                        .UseUrls($"http://localhost:{Port}")
                        .UseConfiguration(config)
                        .UseContentRoot(BasePath);
                })
                .Build();

            await _host.StartAsync();
            IsRunning = true;

            await _host.WaitForShutdownAsync();

            return true;
        }

        private string buildJsonConfig()
        {
            return $"{{ \"basePath\": \"{escapeBackslashes(BasePath)}\" }}";
        }

        private string escapeBackslashes(string value)
        {
            return value.Replace("\\", "\\\\");
        }

        public void Dispose()
        {
            _host?.Dispose();
            IsRunning = false;
        }
    }
}
