using Netyll.Logic.Extensions;
using Netyll.Logic.Templating.Context;

namespace Netyll.Logic.Extensibility
{
    public abstract class TagFactoryBase : DotLiquid.ITagFactory
    {
        private readonly string _tageName;

        protected SiteContext SiteContext { get; private set; }

        protected TagFactoryBase(string tagName)
        {
            _tageName = tagName.ToUnderscoreCase();
        }

        public string TagName
        {
            get
            {
                return _tageName;
            }
        }

        public DotLiquid.Tag Create()
        {
            return (DotLiquid.Tag)CreateTag();
        }

        public abstract ITag CreateTag();

        internal void Initialize(SiteContext siteContext)
        {
            SiteContext = siteContext;
        }
    }
}
