using Netyll.Logic.Templating.Context;

namespace Netyll.Logic.Extensibility
{
    public interface IBeforeProcessingTransform
    {
        void Transform(SiteContext context);
    }
}
