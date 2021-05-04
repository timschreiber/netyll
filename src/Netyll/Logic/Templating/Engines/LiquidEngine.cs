using DotLiquid;
using Netyll.Logic.Exceptions;
using Netyll.Logic.Extensibility;
using Netyll.Logic.Extensions;
using Netyll.Logic.Liquid;
using Netyll.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Netyll.Logic.Templating.Engines
{
    public class LiquidEngine : ISiteEngine //: EngineBase
    {
        private static readonly Regex emHtmlRegex = new Regex(@"(?<=\{[\{\%].*?)(</?em>)(?=.*?[\%\}]\})", RegexOptions.Compiled);
        private static readonly Regex paragraphRegex = new Regex(@"(<(?:p|h\d{1})>.*?</(?:p|h\d{1})>)", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly string[] _layoutExtensions = { ".html", ".htm" };

        private SiteContextDrop contextDrop;
        private SiteContext _context;
        private ILightweightMarkupEngine _lightweightMarkupEngine;
        private IFileSystem _fileSystem;

        static LiquidEngine()
        {
            DotLiquid.Liquid.UseRubyDateFormat = true;
        }

        public LiquidEngine(ILightweightMarkupEngine lightweightMarkupEngine, IFileSystem fileSystem)
//            : base(lightweightMarkupEngine, fileSystem)
        {
            _lightweightMarkupEngine = lightweightMarkupEngine; 
            _fileSystem = fileSystem;
        }

        public IEnumerable<IContentTransform> ContentTransformers { get; set; }
        public IEnumerable<ITag> Tags { get; set; } = new List<ITag>();
        public IEnumerable<IFilter> Filters { get; set; }
        public IEnumerable<TagFactoryBase> TagFactories { get; set; }

        public void Initialize()
        {
            Template.RegisterFilter(typeof(XmlEscapeFilter));
            Template.RegisterFilter(typeof(DateToXmlSchemaFilter));
            Template.RegisterFilter(typeof(DateToStringFilter));
            Template.RegisterFilter(typeof(DateToLongStringFilter));
            Template.RegisterFilter(typeof(DateToRfc822FormatFilter));
            Template.RegisterFilter(typeof(CgiEscapeFilter));
            Template.RegisterFilter(typeof(UriEscapeFilter));
            Template.RegisterFilter(typeof(NumberOfWordsFilter));
            Template.RegisterTag<HighlightBlock>("highlight");
        }

        public void Process(SiteContext context, bool skipFileOnError = false)
        {
            _context = context;
            preProcess();

            for (var i = 0; i < context.Posts.Count; i++)
            {
                var post = context.Posts[i];
                var previousPost = getPrevious(context.Posts, i);
                var nextPost = getNext(context.Posts, i);
                ProcessFile(context.OutputFolder, post, previousPost, nextPost, skipFileOnError, post.Filepath);
            }

            for (var i = 0; i < context.Pages.Count; i++)
            {
                var page = context.Pages[i];
                var previousPage = getPrevious(context.Pages, i);
                var nextPage = getNext(context.Pages, i);
                ProcessFile(context.OutputFolder, page, previousPage, nextPage, skipFileOnError);
            }
        }

        private void preProcess()
        {
            contextDrop = new SiteContextDrop(_context);

            Template.FileSystem = new Includes(_context.SourceFolder, _fileSystem);

            if (Filters != null)
            {
                foreach (var filter in Filters)
                {
                    Template.RegisterFilter(filter.GetType());
                }
            }
            if (Tags != null)
            {
                var registerTagMethod = typeof(Template).GetMethod("RegisterTag", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                foreach (var tag in Tags)
                {
                    var registerTagGenericMethod = registerTagMethod.MakeGenericMethod(new[] { tag.GetType() });
                    registerTagGenericMethod.Invoke(null, new[] { tag.Name.ToUnderscoreCase() });
                }
            }
            if (TagFactories != null)
            {
                foreach (var tagFactory in TagFactories)
                {
                    tagFactory.Initialize(_context);
                    Template.RegisterTagFactory(tagFactory);
                }
            }
        }

        private Hash createPageData(PageContext pageContext)
        {
            var y = Hash.FromDictionary(pageContext.Bag);

            if (y.ContainsKey("title"))
            {
                if (string.IsNullOrWhiteSpace(y["title"].ToString()))
                {
                    y["title"] = _context.Title;
                }
            }
            else
            {
                y.Add("title", _context.Title);
            }

            y.Add("previous", pageContext.Previous);
            y.Add("next", pageContext.Next);

            var x = Hash.FromAnonymousObject(new
            {
                site = contextDrop.ToHash(),
                wtftime = Hash.FromAnonymousObject(new { date = DateTime.Now }),
                page = y,
                content = pageContext.FullContent,
                paginator = pageContext.Paginator,
            });

            return x;
        }

        private string renderTemplate(string content, PageContext pageData)
        {
            // Replace all em HTML tags in liquid tags ({{ or {%) by underscores
            content = emHtmlRegex.Replace(content, "_");

            var data = createPageData(pageData);
            var template = Template.Parse(content);
            var output = template.Render(data);

            return output;
        }

        private static Page getNext(IList<Page> pages, int index) => index >= 1 ? pages[index - 1] : null;
        private static Page getPrevious(IList<Page> pages, int index) => index < pages.Count - 1 ? pages[index + 1] : null;

        private string mapToOutputPath(string file) => file.Replace(_context.SourceFolder.FullName, string.Empty)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        private void createOutputDirectory(string outputFile)
        {
            var directory = Path.GetDirectoryName(outputFile);
            if (!_fileSystem.Directory.Exists(directory))
                _fileSystem.Directory.CreateDirectory(directory);
        }

        private void copyFileIfSourceNewer(string sourceFileName, string destFileName, bool overwrite)
        {
            if (!_fileSystem.File.Exists(destFileName)
                || _fileSystem.File.GetLastWriteTime(sourceFileName) > _fileSystem.File.GetLastWriteTime(destFileName))
            {
                _fileSystem.File.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        private string renderContent(string file, string contents)
        {
            string html;
            try
            {
                var contentsWithoutHeader = contents.ExcludeHeader();

                html = Path.GetExtension(file).IsMarkdownFile()
                       ? _lightweightMarkupEngine.Convert(contentsWithoutHeader).Trim()
                       : contentsWithoutHeader;

                if (ContentTransformers != null)
                {
                    html = ContentTransformers.Aggregate(html, (current, contentTransformer) => contentTransformer.Transform(current));
                }
            }
            catch (Exception e)
            {
                // Log the exception Serilog?

                //Tracing.Info("Error ({0}) converting {1}", e.Message, file);
                //Tracing.Debug(e.ToString());
                html = String.Format("<p><b>Error converting markdown:</b><br />{0}</p><p>Original content:<br /><pre>{1}</pre></p>", e.Message, contents);
            }
            return html;
        }

        private static string GetContentExcerpt(string content, string excerptSeparator)
        {
            var excerptSeparatorIndex = content.IndexOf(excerptSeparator, StringComparison.InvariantCulture);
            string excerpt = null;
            if (excerptSeparatorIndex == -1)
            {
                var match = paragraphRegex.Match(content);
                if (match.Success)
                {
                    excerpt = match.Groups[1].Value;
                }
            }
            else
            {
                excerpt = content.Substring(0, excerptSeparatorIndex);
                if (excerpt.StartsWith("<p>") && !excerpt.EndsWith("</p>"))
                {
                    excerpt += "</p>";
                }
            }
            return excerpt;
        }

        private string FindLayoutPath(string layout)
        {
            foreach (var extension in _layoutExtensions)
            {
                var path = Path.Combine(_context.SourceFolder.FullName, "_layouts", layout + extension);
                if (_fileSystem.File.Exists(path))
                    return path;
            }

            return null;
        }

        private IDictionary<string, object> processTemplate(PageContext pageContext, string path)
        {
            var templateFile = _fileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            pageContext.FullContent = renderTemplate(templateContent, pageContext);

            return metadata;
        }

        private void ProcessFile(IDirectoryInfo outputDirectory, Page page, Page previous, Page next, bool skipFileOnError, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = mapToOutputPath(page.File);

            page.OutputFile = Path.Combine(outputDirectory.FullName, relativePath);
            var extension = Path.GetExtension(page.File);

            if (extension.IsImageFormat())
            {
                createOutputDirectory(page.OutputFile);
                copyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }

            if (page is NonProcessedPage)
            {
                createOutputDirectory(page.OutputFile);
                copyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }

            if (extension.IsMarkdownFile())
            {
                page.OutputFile = page.OutputFile.Replace(extension, ".html");
            }

            var pageContext = PageContext.FromPage(_context, page, outputDirectory, page.OutputFile);

            pageContext.Previous = previous;
            pageContext.Next = next;

            var pageContexts = new List<PageContext> { pageContext };
            object paginateObj;
            if (page.Bag.TryGetValue("paginate", out paginateObj))
            {
                var paginate = Convert.ToInt32(paginateObj);
                var totalPages = (int)Math.Ceiling(_context.Posts.Count / Convert.ToDouble(paginateObj));
                var paginator = new Paginator(_context, totalPages, paginate, 1);
                pageContext.Paginator = paginator;

                var paginateLink = "/page/:page/index.html";
                if (page.Bag.ContainsKey("paginate_link"))
                    paginateLink = Convert.ToString(page.Bag["paginate_link"]);

                var prevLink = page.Url;
                for (var i = 2; i <= totalPages; i++)
                {
                    var newPaginator = new Paginator(_context, totalPages, paginate, i) { PreviousPageUrl = prevLink };
                    var link = paginateLink.Replace(":page", Convert.ToString(i));
                    paginator.NextPageUrl = link;

                    paginator = newPaginator;
                    prevLink = link;

                    var path = Path.Combine(outputDirectory.FullName, link.ToRelativeFile());
                    if (path.EndsWith(_fileSystem.Path.DirectorySeparatorChar.ToString()))
                    {
                        path = Path.Combine(path, "index.html");
                    }
                    var context = new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path };
                    context.Bag["url"] = link;
                    pageContexts.Add(context);
                }
            }

            foreach (var context in pageContexts)
            {
                var metadata = page.Bag;
                var failed = false;

                var excerptSeparator = context.Bag.ContainsKey("excerpt_separator")
                    ? context.Bag["excerpt_separator"].ToString()
                    : _context.ExcerptSeparator;
                try
                {
                    context.Content = renderContent(page.File, renderTemplate(context.Content, context));
                    context.FullContent = context.Content;
                    context.Bag["excerpt"] = GetContentExcerpt(context.Content, excerptSeparator);
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process {0}, see inner exception for more details", context.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process {0}: {1}", context.OutputPath, ex);
                    continue;
                }

                while (metadata.ContainsKey("layout"))
                {
                    var layout = metadata["layout"];
                    if ((string)layout == "nil" || layout == null)
                        break;

                    var path = FindLayoutPath(layout.ToString());

                    if (path == null)
                        break;

                    try
                    {
                        metadata = processTemplate(context, path);
                    }
                    catch (Exception ex)
                    {
                        if (!skipFileOnError)
                        {
                            var message = string.Format("Failed to process layout {0} for {1}, see inner exception for more details", layout, context.OutputPath);
                            throw new PageProcessingException(message, ex);
                        }

                        Console.WriteLine(@"Failed to process layout {0} for {1} because '{2}'. Skipping file", layout, context.OutputPath, ex.Message);
                        failed = true;
                        break;
                    }
                }
                if (failed)
                {
                    continue;
                }

                createOutputDirectory(context.OutputPath);
                _fileSystem.File.WriteAllText(context.OutputPath, context.FullContent);
            }
        }
    }
}
