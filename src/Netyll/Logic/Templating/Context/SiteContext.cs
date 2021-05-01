using System;
using System.Collections.Generic;
using System.Linq;

namespace Netyll.Logic.Templating.Context
{
    public class SiteContext
    {
        private const string EXCERPT_SEPARATOR_DEFAULT = "<!--more-->";

        private string engine;
        private string title;
        private string excerptSeparator = EXCERPT_SEPARATOR_DEFAULT;

        public SiteContext()
        {
            Tags = new List<Tag>();
            Categories = new List<Category>();
            Posts = new List<Page>();
            Pages = new List<Page>();
            Config = new Configuration();
        }

        public IConfiguration Config { get; set; }
        public string SourceFolder { get; set; }
        public string OutputFolder { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IList<Page> Posts { get; set; }
        public DateTime Time { get; set; }
        public bool UseDrafts { get; set; }
        public List<Page> Pages { get; set; }
        public Data Data { get; set; }
        public List<Page> Html_Pages => Pages.Where(p => p.Url != null && p.Url.EndsWith(".html")).ToList();

        public string Title
        {
            get
            {
                if (Config.ContainsKey("title"))
                    title = Config["title"].ToString();
                return title;
            }
            set { title = value; }
        }

        public string ExcerptSeparator
        {
            get
            {
                if (Config.ContainsKey("excerpt_separator"))
                    excerptSeparator = Config["excerpt_separator"].ToString();
                return excerptSeparator;
            }
        }

        public string Engine
        {
            get
            {
                if (engine == null)
                {
                    if (!Config.ContainsKey("netyll"))
                    {
                        engine = string.Empty;
                        return engine;
                    }

                    var netyllSettings = Config["netyll"] as Dictionary<string, object>;

                    engine = netyllSettings != null && netyllSettings.ContainsKey("engine")
                        ? (string)netyllSettings["engine"]
                        : string.Empty;
                }

                return engine;
            }
        }
    }
}
