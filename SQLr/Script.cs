using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SQLr
{
    public class Script
    {
        public Script(string scriptFilePath = null)
        {
            _ordinal = long.MaxValue;

            if (!string.IsNullOrEmpty(scriptFilePath))
            {
                if (!Constants.ScriptRegex.IsMatch(scriptFilePath))
                    throw new ArgumentException("The file name given is not valid in the conversion application.");

                FilePath = scriptFilePath;
            }
        }

        #region Timeout

        private string _timeout;

        public int GetTimeout(Dictionary<string, string> variableMapping = null)
        {
            if (_timeout == null)
                return 6000;

            string mappedTime = _timeout;
            if (variableMapping != null)
                mappedTime = variableMapping[_timeout.TrimStart('<').TrimEnd('>')] ?? _timeout;

            int time;
            if (!int.TryParse(mappedTime, out time))
                return 6000;

            return time;
        }

        #endregion Timeout

        #region Warning

        private string _warning;

        public string GetWarning(Dictionary<string, string> variableMapping = null)
        {
            if (_warning == null)
                return null;

            if (variableMapping != null)
                return variableMapping[_warning.TrimStart('<').TrimEnd('>')] ?? _warning;

            return _warning;
        }

        #endregion Warning

        #region Variables

        public IEnumerable<string> Variables { get; private set; }

        #endregion Variables

        #region Subsets

        private List<string> _subsets;

        public IEnumerable<string> GetSubsets(Dictionary<string, string> variableMapping = null)
        {
            if (_subsets == null)
                return new List<string>();

            if (variableMapping != null)
                return _subsets.Select(s => variableMapping[s.TrimStart('<').TrimEnd('>')] ?? s);

            return _subsets;
        }

        #endregion Subsets

        #region Database

        private string _database;

        public string GetDatabase(Dictionary<string, string> variableMapping = null)
        {
            if (_database == null)
                return null;

            if (variableMapping != null)
                return variableMapping[_database.TrimStart('<').TrimEnd('>')] ?? _database;

            return _database;
        }

        #endregion Database

        #region FilePath

        private string _filePath;

        public string FilePath
        {
            get
            {
                return _filePath;
            }

            set
            {
                _filePath = value;

                if (!File.Exists(_filePath))
                    throw new FileNotFoundException("Could not find the expected file", _filePath);

                var match = Constants.ScriptRegex.Match(_filePath);

                // Set Name
                _name = match.Groups[2].Value;

                // Set Ordinal
                long ordinal;
                if (long.TryParse(match.Groups[1].Value, out ordinal))
                    _ordinal = ordinal;
                else
                    throw new Exception("The ordinal did not parse correctly");

                Text = File.ReadAllText(_filePath);
            }
        }

        #endregion FilePath

        #region Name

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_filePath != null)
                    throw new InvalidOperationException($"Cannot set the name on a file-backed Script object. The script is backed by the file ({_filePath})");

                _name = value;
            }
        }

        #endregion Name

        #region Ordinal

        private long _ordinal;

        public long Ordinal
        {
            get { return _ordinal; }
            set
            {
                if (_filePath != null)
                    throw new InvalidOperationException($"Cannot set the ordinal on a file-backed Script object. The script is backed by the file ({_filePath})");

                _ordinal = value;
            }
        }

        #endregion Ordinal

        #region Text

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RefreshMetadata();
            }
        }

        #endregion Text

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Script)) return false;
            var other = obj as Script;
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public void RereadFile()
        {
            Text = File.ReadAllText(_filePath);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public string GetVariableReplacedQuery(Dictionary<string, string> variableReplacements)
        {
            var missing = Variables.Except(variableReplacements.Keys).ToList();
            if (missing.Any())
                throw new Exception($"Missing variables: {missing.Aggregate("", (c, n) => c + n + ", ").TrimEnd(' ', ',')}");

            var query = new StringBuilder(Text);
            foreach (var pair in variableReplacements)
            {
                query.Replace("<<" + pair.Key + ">>", pair.Value);
            }

            return query.ToString();
        }

        private void RefreshMetadata()
        {
            Variables = LoadMultiTag(_text, @"<<(\w+?)>>").Distinct();

            _subsets = LoadMultiTag(_text, @"{{Subset=(\w+?)}}").ToList();

            _timeout = LoadTag(_text, @"{{Timeout=(\d+?)}}") ?? "6000";

            _warning = LoadTag(_text, @"{{Warning=(.+?)}}");

            _database = LoadTag(_text, @"{{Database=(.+?)}}");
        }

        private static IEnumerable<string> LoadMultiTag(string text, string regex, int grouping = 1)
        {
            var re = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var matchCollection = re.Matches(text);

            return (from Match match in matchCollection select match.Groups[grouping].Value).Distinct();
        }

        private string LoadTag(string text, string regex)
        {
            var re = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var matchCollection = re.Matches(text);

            if (matchCollection.Count == 0)
                return null;

            var tag = (from Match match in matchCollection select match.Groups[1].Value).FirstOrDefault();

            return tag?.Replace("--", "");
        }
    }
}