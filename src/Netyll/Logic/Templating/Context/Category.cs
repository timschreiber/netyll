using DotLiquid;
using System.Collections.Generic;

namespace Netyll.Logic.Templating.Context
{
    public class Category : Drop
    {
        public IEnumerable<Page> Posts { get; set; }
        public string Name { get; set; }
    }
}
