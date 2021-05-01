﻿using DotLiquid;
using Netyll.Logic.Extensions;
using Netyll.Logic.Liquid;
using Netyll.Logic.Templating.Context;
using System;
using System.Text.RegularExpressions;

namespace Netyll.Logic.Templating.Engines
{
    public class LiquidEngine : EngineBase
    {
        private SiteContextDrop contextDrop;
        private static readonly Regex emHtmlRegex = new Regex(@"(?<=\{[\{\%].*?)(</?em>)(?=.*?[\%\}]\})", RegexOptions.Compiled);

        static LiquidEngine()
        {
            DotLiquid.Liquid.UseRubyDateFormat = true;
        }

        protected override void PreProcess()
        {
            contextDrop = new SiteContextDrop(Context);

            Template.FileSystem = new Includes(Context.SourceFolder, FileSystem);

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
                    tagFactory.Initialize(Context);
                    Template.RegisterTagFactory(tagFactory);
                }
            }
        }

        private Hash CreatePageData(PageContext pageContext)
        {
            var y = Hash.FromDictionary(pageContext.Bag);

            if (y.ContainsKey("title"))
            {
                if (string.IsNullOrWhiteSpace(y["title"].ToString()))
                {
                    y["title"] = Context.Title;
                }
            }
            else
            {
                y.Add("title", Context.Title);
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

        protected override string RenderTemplate(string content, PageContext pageData)
        {
            // Replace all em HTML tags in liquid tags ({{ or {%) by underscores
            content = emHtmlRegex.Replace(content, "_");

            var data = CreatePageData(pageData);
            var template = Template.Parse(content);
            var output = template.Render(data);

            return output;
        }

        public override void Initialize()
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
    }
}
