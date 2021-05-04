using Netyll.Logic.Templating.Context;
using Netyll.Logic.Templating.Engines;
using System.IO.Abstractions;

namespace Netyll.Logic.Extensions
{
    public static class Sitemap
    {
        public static void CompressSitemap(this ISiteEngine engine, SiteContext siteContext, IFileSystem fileSystem)
        {
            var sitemap = fileSystem.Path.Combine(siteContext.OutputFolder.FullName, @"sitemap.xml");
            var compressedSitemap = sitemap + ".gz";

            if (fileSystem.File.Exists(sitemap))
            {
                using (var sitemapStream = fileSystem.File.OpenRead(sitemap))
                {
                    using (var compressedMap = fileSystem.File.Create(compressedSitemap))
                    {
                        using (var gzip = new System.IO.Compression.GZipStream(compressedMap, System.IO.Compression.CompressionLevel.Optimal))
                        {
                            sitemapStream.CopyTo(gzip);
                        }
                    }
                }
            }
        }
    }
}
