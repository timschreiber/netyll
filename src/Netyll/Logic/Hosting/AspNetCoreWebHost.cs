using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace Netyll.Logic.Hosting
{
    public class AspNetCoreWebHost : IDisposable
    {
        private IHost _host;

        public AspNetCoreWebHost(IDirectoryInfo basePath)
            : this(basePath, 8080)
        { }

        public AspNetCoreWebHost(IDirectoryInfo basePath, int port)
        {
            IsRunning = false;
            BasePath = basePath == null ? Environment.CurrentDirectory : basePath.FullName;
            Port = port;
        }

        public bool IsRunning { get; private set; }
        public string BasePath { get; }
        public int Port { get; }

        public async Task<bool> Start()
        {
            if (IsRunning)
                return false;

            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging(logging => logging.AddConsole());
                    webBuilder.UseWebRoot(BasePath);
                    webBuilder.UseUrls($"http://localhost:{Port}");
                })
                .Build();

            await _host.StartAsync();
            IsRunning = true;
            return true;
        }

        public async Task<bool> Stop()
        {
            if (!IsRunning)
                return false;

            await _host.StopAsync();
            _host.Dispose();
            _host = null;

            return true;
        }

        public void Dispose()
        {
            _host?.Dispose();
            IsRunning = false;
        }
    }
}
