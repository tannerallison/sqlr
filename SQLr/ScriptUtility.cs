// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - ScriptUtility.cs
// <Author></Author>
// <CreatedDate>2016-09-24</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    #region using

    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    #endregion

    public static class ScriptUtility
    {
        public static IEnumerable<string> LoadMultiTag(string text, string regex, int grouping = 1)
        {
            var re = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var matchCollection = re.Matches(text);

            return (from Match match in matchCollection select match.Groups[grouping].Value).Distinct();
        }

        public static string LoadTag(string text, string regex)
        {
            var re = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var matchCollection = re.Matches(text);

            if (matchCollection.Count == 0)
                return null;

            var tag = (from Match match in matchCollection select match.Groups[1].Value).FirstOrDefault();

            return tag?.Replace("--", string.Empty);
        }
    }
}