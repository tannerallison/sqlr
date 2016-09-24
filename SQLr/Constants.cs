using System.Text.RegularExpressions;

namespace SQLr
{
    internal class Constants
    {
        public static readonly Regex ScriptRegex = new Regex("_(\\d+)_(.*)\\.sql$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }
}