using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Netyll.Logic.Templating.Context
{
    public class SiteContext
    {
        private const string EXCERPT_SEPARATOR_DEFAULT = "<!--more-->";

        private string _title;
        private string _excerptSeparator = EXCERPT_SEPARATOR_DEFAULT;

        public IDirectoryInfo SourceFolder { get; set; }
        public IDirectoryInfo OutputFolder { get; set; }
        public bool UseDrafts { get; set; }

        public IConfiguration Config { get; set; } = new Configuration();
        public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IList<Page> Posts { get; set; } = new List<Page>();
        public List<Page> Pages { get; set; } = new List<Page>();

        public DateTime Time { get; set; }
        public Data Data { get; set; }
        public List<Page> Html_Pages => Pages.Where(p => p.Url != null && p.Url.EndsWith(".html")).ToList();

        public string Title
        {
            get
            {
                if (Config.ContainsKey("title"))
                    _title = Config["title"].ToString();
                return _title;
            }
            set { _title = value; }
        }

        public string ExcerptSeparator
        {
            get
            {
                if (Config.ContainsKey("excerpt_separator"))
                    _excerptSeparator = Config["excerpt_separator"].ToString();
                return _excerptSeparator;
            }
        }
    }
}
