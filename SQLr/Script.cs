// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - Script.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    #endregion

    public class Script : IProcessStep
    {
        private string database;
        private string filePath;
        private string name;
        private long ordinal;
        private List<string> subsets;
        private string text;
        private string timeout;
        private string warning;

        public Script(string scriptFilePath = null)
        {
            ordinal = long.MaxValue;

            if (!string.IsNullOrEmpty(scriptFilePath))
            {
                if (!Constants.ScriptRegex.IsMatch(scriptFilePath))
                    throw new ArgumentException("The file name given is not valid in the conversion application.");

                FilePath = scriptFilePath;
            }
        }

        /// <summary>
        ///     Gets or sets the FilePath of the script. When the FilePath is set the Name and Ordinal
        ///     fields are set from the path and the text of the file is automatically written into Text
        ///     which automatically refreshes all the other fields as well
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;

                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Could not find the expected file", filePath);

                var match = Constants.ScriptRegex.Match(filePath);

                // Set Name
                name = match.Groups[2].Value;

                // Set Ordinal
                long tempOrd;
                if (long.TryParse(match.Groups[1].Value, out tempOrd))
                    ordinal = tempOrd;
                else
                    throw new Exception("The ordinal did not parse correctly");

                Text = File.ReadAllText(filePath);
            }
        }

        /// <summary>
        ///     Gets or sets the name of the script.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If an attempt is made to set the name when the name is pulled from the FilePath.
        /// </exception>
        public string Name
        {
            get { return name; }
            set
            {
                if (filePath != null)
                {
                    throw new InvalidOperationException(
                              $"Cannot set the name on a file-backed Script object. The script is backed by the file ({filePath})");
                }

                name = value;
            }
        }

        /// <summary>
        ///     Gets or sets the ordinal.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If an attempt is made to set the ordinal when the ordinal is pulled from the FilePath.
        /// </exception>
        public long Ordinal
        {
            get { return ordinal; }
            set
            {
                if (filePath != null)
                {
                    throw new InvalidOperationException(
                              $"Cannot set the ordinal on a file-backed Script object. The script is backed by the file ({filePath})");
                }

                ordinal = value;
            }
        }

        public void HasChanges()
        {
            RereadFile();
        }

        /// <summary>
        ///     Sets the text of the script and automatically updates the Variables, Subsets, Database,
        ///     Timeout, and Warning fields are automatically refreshed
        /// </summary>
        public string Text
        {
            set
            {
                text = value;
                RefreshMetadata();
            }
        }

        /// <summary>
        ///     Returns the variables within the script.
        /// </summary>
        public IEnumerable<string> Variables { get; private set; }

        /// <summary>Returns the Database metatag of the Script, if there is one.</summary>
        /// <param name="variableMapping">The variable mapping.</param>
        /// <returns>The Database to run the script against.</returns>
        public string GetDatabase(Dictionary<string, string> variableMapping = null)
        {
            if (database == null)
                return null;

            if (variableMapping != null)
                return variableMapping[database.TrimStart('<').TrimEnd('>')] ?? database;

            return database;
        }

        /// <summary>
        /// </summary>
        /// <param name="variableMapping"></param>
        /// <returns></returns>
        public IEnumerable<string> GetSubsets(Dictionary<string, string> variableMapping = null)
        {
            if (subsets == null)
                return new List<string>();

            if (variableMapping != null)
                return subsets.Select(s => variableMapping[s.TrimStart('<').TrimEnd('>')] ?? s);

            return subsets;
        }

        /// <summary>
        /// </summary>
        /// <param name="variableMapping"></param>
        /// <returns></returns>
        public string GetText(Dictionary<string, string> variableMapping = null)
        {
            if (text == null)
                return null;

            var returnText = new StringBuilder(text);
            if (variableMapping != null)
            {
                var missing = Variables.Except(variableMapping.Keys).ToList();
                if (missing.Any())
                {
                    throw new ArgumentException(
                              $"Missing variables: {missing.Aggregate(string.Empty, (c, n) => c + n + ", ").TrimEnd(' ', ',')}");
                }

                foreach (var v in variableMapping)
                    returnText.Replace($"<<{v.Key}>>", v.Value);
            }

            return returnText.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="variableMapping"></param>
        /// <returns></returns>
        public int GetTimeout(Dictionary<string, string> variableMapping = null)
        {
            if (timeout == null)
                return 6000;

            var mappedTime = timeout;
            if (variableMapping != null)
                mappedTime = variableMapping[timeout.TrimStart('<').TrimEnd('>')] ?? timeout;

            int time;
            if (!int.TryParse(mappedTime, out time))
                return 6000;

            return time;
        }

        /// <summary>
        /// </summary>
        /// <param name="variableMapping"></param>
        /// <returns></returns>
        public string GetWarning(Dictionary<string, string> variableMapping = null)
        {
            if (warning == null)
                return null;

            if (variableMapping != null)
                return variableMapping[warning.TrimStart('<').TrimEnd('>')] ?? warning;

            return warning;
        }

        /// <summary>
        ///     Causes the content of the file in the FilePath to be re-read to the Text field.
        /// </summary>
        public void RereadFile()
        {
            Text = File.ReadAllText(filePath);
        }

        private void RefreshMetadata()
        {
            Variables = ScriptUtility.LoadMultiTag(text, @"<<(\w+?)>>").Distinct();

            subsets = ScriptUtility.LoadMultiTag(text, @"{{Subset=([\w<>]+?)}}").ToList();

            timeout = ScriptUtility.LoadTag(text, @"{{Timeout=([\w<>]+?)}}") ?? "6000";

            warning = ScriptUtility.LoadTag(text, @"{{Warning=(.+?)}}");

            database = ScriptUtility.LoadTag(text, @"{{Database=(.+?)}}");
        }
    }
}