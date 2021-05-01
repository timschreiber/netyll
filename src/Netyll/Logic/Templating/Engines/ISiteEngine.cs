using Netyll.Logic.Templating.Context;

namespace Netyll.Logic.Templating.Engines
{
    public interface ISiteEngine
    {
        void Initialize();

        bool CanProcess(SiteContext context);

        void Process(SiteContext context, bool skipFileOnError = false);
    }
}
