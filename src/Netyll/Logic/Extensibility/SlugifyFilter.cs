using System.Globalization;
using System.Text.RegularExpressions;

namespace Netyll.Logic.Extensibility
{
    public class SlugifyFilter : IFilter
    {
        public string Name
        {
            get { return "Slugify"; }
        }

        public static string Slugify(string input)
        {
            var str = input.ToLower(CultureInfo.InvariantCulture).Trim('.', ' ');

            str = str.Replace("#", "sharp").Replace('.', '-');
            // invalid chars, make into spaces
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces/hyphens into one space
            str = Regex.Replace(str, @"[\s-]+", " ").Trim();
            // hyphens
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }
    }
}
