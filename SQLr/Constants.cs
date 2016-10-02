// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - Constants.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-10-01</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    #region using

    using System.Text.RegularExpressions;

    #endregion

    internal class Constants
    {
        public static readonly Regex ScriptRegex = new Regex(
                                                       "_(\\d+)_(.*)\\.sql$",
                                                       RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }
}