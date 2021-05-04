using Netyll.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Linq;
using Netyll.Logic.Extensions;
using System.Globalization;

namespace Netyll.Logic.Templating.Context
{
    public class SiteContextGenerator
    {
        private readonly Dictionary<string, Page> _pageCache = new Dictionary<string, Page>();
        private readonly IFileSystem _fileSystem;
        private readonly List<string> _includes = new List<string>();
        private readonly List<string> _excludes = new List<string>();
        private readonly LinkHelper _linkHelper;
        private readonly IConfiguration _config;

        public IEnumerable<IBeforeProcessingTransform> BeforeProcessingTransforms { get; set; }

        public SiteContextGenerator(IFileSystem fileSystem, LinkHelper linkHelper, IConfiguration config)
        {
            _fileSystem = fileSystem;
            _linkHelper = linkHelper;
            _config = config;

            if (_config.ContainsKey("include"))
            {
                _includes.AddRange((IEnumerable<string>)_config["include"]);
            }
            if (_config.ContainsKey("exclude"))
            {
                _excludes.AddRange((IEnumerable<string>)_config["exclude"]);
            }
        }

        public SiteContext BuildContext(IDirectoryInfo sourcePath, IDirectoryInfo destinationPath, bool includeDrafts)
        {
            try
            {
                _config.ReadFromFile(sourcePath);

                var context = new SiteContext
                {
                    SourceFolder = sourcePath,
                    OutputFolder = destinationPath,
                    Posts = new List<Page>(),
                    Pages = new List<Page>(),
                    Config = _config,
                    Time = DateTime.Now,
                    UseDrafts = includeDrafts,
                    Data = new Data(_fileSystem, _fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(sourcePath.FullName, "_data")))
                };

                context.Posts = BuildPosts(_config, context).OrderByDescending(p => p.Date).ToList();
                BuildTagsAndCategories(context);

                context.Pages = BuildPages(_config, context).ToList();

                if (BeforeProcessingTransforms != null)
                {
                    foreach (var transform in BeforeProcessingTransforms)
                    {
                        transform.Transform(context);
                    }
                }

                return context;
            }
            finally
            {
                _pageCache.Clear();
            }
        }

        private IEnumerable<Page> BuildPages(IConfiguration config, SiteContext context)
        {
            var files = from file in _fileSystem.Directory.GetFiles(context.SourceFolder.FullName, "*.*", SearchOption.AllDirectories)
                        let relativePath = MapToOutputPath(context, file)
                        where CanBeIncluded(relativePath)
                        select file;

            foreach (var file in files)
            {
                if (!ContainsYamlFrontMatter(file))
                {
                    yield return new NonProcessedPage
                    {
                        File = file,
                        Filepath = Path.Combine(context.OutputFolder.FullName, MapToOutputPath(context, file))
                    };
                }
                else
                {
                    var page = CreatePage(context, config, file, false);

                    if (page != null)
                        yield return page;
                }
            }
        }
        private IEnumerable<Page> BuildPosts(IConfiguration config, SiteContext context)
        {
            var posts = new List<Page>();

            var postsFolders = _fileSystem.Directory.GetDirectories(context.SourceFolder.FullName, "_posts", SearchOption.AllDirectories);

            foreach (var postsFolder in postsFolders)
            {
                posts.AddRange(_fileSystem.Directory
                    .GetFiles(postsFolder, "*.*", SearchOption.AllDirectories)
                    .Select(file => CreatePage(context, config, file, true))
                    .Where(post => post != null)
                );
            }

            var draftsFolder = Path.Combine(context.SourceFolder.FullName, "_drafts");
            if (context.UseDrafts && _fileSystem.Directory.Exists(draftsFolder))
            {
                posts.AddRange(_fileSystem.Directory
                    .GetFiles(draftsFolder, "*.*", SearchOption.AllDirectories)
                    .Select(file => CreatePage(context, config, file, true))
                    .Where(post => post != null)
                );
            }

            return posts;
        }

        private static void BuildTagsAndCategories(SiteContext context)
        {
            var tags = new Dictionary<string, List<Page>>();
            var categories = new Dictionary<string, List<Page>>();

            foreach (var post in context.Posts)
            {
                if (post.Tags != null)
                {
                    foreach (var tagName in post.Tags)
                    {
                        if (tags.ContainsKey(tagName))
                        {
                            tags[tagName].Add(post);
                        }
                        else
                        {
                            tags.Add(tagName, new List<Page> { post });
                        }
                    }
                }

                if (post.Categories != null)
                {
                    foreach (var categoryName in post.Categories)
                    {
                        AddCategory(categories, categoryName, post);
                    }
                }
            }

            context.Tags = tags.Select(x => new Tag { Name = x.Key, Posts = x.Value }).OrderBy(x => x.Name).ToList();
            context.Categories = categories.Select(x => new Category { Name = x.Key, Posts = x.Value }).OrderBy(x => x.Name).ToList();
        }

        private static void AddCategory(Dictionary<string, List<Page>> categories, string categoryName, Page post)
        {
            if (categories.ContainsKey(categoryName))
            {
                categories[categoryName].Add(post);
            }
            else
            {
                categories.Add(categoryName, new List<Page> { post });
            }
        }

        private bool ContainsYamlFrontMatter(string file)
        {
            var postFirstLine = SafeReadLine(file);

            return postFirstLine != null && postFirstLine.StartsWith("---");
        }

        public bool IsExcludedPath(string relativePath)
        {
            return _excludes.Contains(relativePath) || _excludes.Any(e => relativePath.StartsWith(e));
        }

        public bool IsIncludedPath(string relativePath)
        {
            return _includes.Contains(relativePath) || _includes.Any(e => relativePath.StartsWith(e));
        }

        public bool CanBeIncluded(string relativePath)
        {
            if (_excludes.Count > 0 && IsExcludedPath(relativePath))
            {
                return false;
            }

            if (_includes.Count > 0 && IsIncludedPath(relativePath))
            {
                return true;
            }

            return !IsSpecialPath(relativePath);
        }

        public static bool IsSpecialPath(string relativePath)
        {
            return relativePath.StartsWith("_")
                    || relativePath.Contains("_posts")
                    || (relativePath.StartsWith(".") && relativePath != ".htaccess")
                    || relativePath.EndsWith(".TMP", StringComparison.OrdinalIgnoreCase);
        }

        private Page CreatePage(SiteContext context, IConfiguration config, string file, bool isPost)
        {
            try
            {
                if (_pageCache.ContainsKey(file))
                    return _pageCache[file];
                var content = SafeReadContents(file);

                var relativePath = MapToOutputPath(context, file);
                var scopedDefaults = context.Config.Defaults.ForScope(relativePath);

                var header = scopedDefaults.Merge(content.YamlHeader());

                if (header.ContainsKey("published") && header["published"].ToString().ToLower(CultureInfo.InvariantCulture) == "false")
                {
                    return null;
                }

                var page = new Page
                {
                    Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post",
                    Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(_fileSystem),
                    Content = content,
                    Filepath = isPost ? GetPathWithTimestamp(context.OutputFolder.FullName, file) : GetFilePathForPage(context, file),
                    File = file,
                    Bag = header,
                };

                // resolve categories and tags
                if (isPost)
                {
                    page.Categories = ResolveCategories(context, header, page);

                    if (header.ContainsKey("tags"))
                        page.Tags = header["tags"] as IEnumerable<string>;
                }

                // resolve permalink
                if (header.ContainsKey("permalink"))
                {
                    page.Url = _linkHelper.EvaluatePermalink(header["permalink"].ToString(), page);
                }
                else if (isPost && config.ContainsKey("permalink"))
                {
                    page.Url = _linkHelper.EvaluatePermalink(config["permalink"].ToString(), page);
                }
                else
                {
                    page.Url = _linkHelper.EvaluateLink(context, page);
                }

                // resolve id
                page.Id = page.Url.Replace(".html", string.Empty).Replace("index", string.Empty);

                // always write date back to Bag as DateTime
                page.Bag["date"] = page.Date;

                // The GetDirectoryPage method is reentrant, we need a cache to stop a stack overflow :)
                _pageCache.Add(file, page);
                page.DirectoryPages = GetDirectoryPages(context, config, Path.GetDirectoryName(file), isPost).ToList();

                return page;
            }
            catch// (Exception e)
            {
                // Log Exception Serilog?
            }

            return null;
        }

        private List<string> ResolveCategories(SiteContext context, IDictionary<string, object> header, Page page)
        {
            var categories = new List<string>();

            if (!IsOnlyFrontmatterCategories(context))
            {
                var postPath = page.File.Replace(context.SourceFolder.FullName, string.Empty);
                string rawCategories = postPath.Replace(_fileSystem.Path.GetFileName(page.File), string.Empty).Replace("_posts", string.Empty);
                categories.AddRange(rawCategories.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));
            }
            if (header.ContainsKey("categories") && header["categories"] is IEnumerable<string>)
            {
                categories.AddRange((IEnumerable<string>)header["categories"]);
            }
            else if (header.ContainsKey("category"))
            {
                categories.Add((string)header["category"]);
            }

            return categories;
        }

        private static bool IsOnlyFrontmatterCategories(SiteContext context)
        {
            object onlyFrontmatterCategories;
            if (!context.Config.TryGetValue("only_frontmatter_categories", out onlyFrontmatterCategories))
            {
                return false;
            }
            return onlyFrontmatterCategories is bool && (bool)onlyFrontmatterCategories;
        }

        private string GetFilePathForPage(SiteContext context, string file)
        {
            return Path.Combine(context.OutputFolder.FullName, MapToOutputPath(context, file));
        }

        private IEnumerable<Page> GetDirectoryPages(SiteContext context, IConfiguration config, string forDirectory, bool isPost)
        {
            return _fileSystem
                .Directory
                .GetFiles(forDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Select(file => CreatePage(context, config, file, isPost))
                .Where(page => page != null);
        }

        private string SafeReadLine(string file)
        {
            string postFirstLine;
            try
            {
                using (var reader = _fileSystem.File.OpenText(file))
                {
                    postFirstLine = reader.ReadLine();
                }
            }
            catch (IOException)
            {
                var fileInfo = _fileSystem.FileInfo.FromFileName(file);
                var tempFile = Path.Combine(Path.GetTempPath(), fileInfo.Name);
                try
                {
                    fileInfo.CopyTo(tempFile, true);
                    using (var streamReader = _fileSystem.File.OpenText(tempFile))
                    {
                        return streamReader.ReadLine();
                    }
                }
                finally
                {
                    if (_fileSystem.File.Exists(tempFile))
                        _fileSystem.File.Delete(tempFile);
                }
            }
            return postFirstLine;
        }

        private string SafeReadContents(string file)
        {
            try
            {
                return _fileSystem.File.ReadAllText(file);
            }
            catch (IOException)
            {
                var fileInfo = _fileSystem.FileInfo.FromFileName(file);
                var tempFile = Path.Combine(Path.GetTempPath(), fileInfo.Name);
                try
                {
                    fileInfo.CopyTo(tempFile, true);
                    return _fileSystem.File.ReadAllText(tempFile);
                }
                finally
                {
                    if (_fileSystem.File.Exists(tempFile))
                        _fileSystem.File.Delete(tempFile);
                }
            }
        }

        // http://stackoverflow.com/questions/6716832/sanitizing-string-to-url-safe-format
        public static string RemoveDiacritics(string strThis)
        {
            if (strThis == null)
                return null;

            strThis = strThis.ToLowerInvariant();

            var sb = new StringBuilder();

            foreach (char c in strThis.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private string MapToOutputPath(SiteContext context, string file)
        {
            return file.Replace(context.SourceFolder.FullName, string.Empty)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private string GetPathWithTimestamp(string outputDirectory, string file)
        {
            // TODO: detect mode from site config
            var fileName = Path.GetFileName(file);

            var tokens = fileName.Split('-');
            var timePath = Path.Combine(tokens);
            return Path.Combine(outputDirectory, timePath);
        }
    }
}
