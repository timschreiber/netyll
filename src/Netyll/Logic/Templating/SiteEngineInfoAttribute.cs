using Netyll.Logic.Templating.Engines;
using System;
using System.Composition;

namespace Netyll.Logic.Templating
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SiteEngineInfoAttribute : ExportAttribute
    {
        public string Engine { get; set; }

        public SiteEngineInfoAttribute() : base(typeof(ISiteEngine))
        {
        }
    }
}
