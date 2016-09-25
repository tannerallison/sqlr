using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SQLr
{
    [System.Runtime.InteropServices.Guid("86243A08-834F-42EC-AA4F-97D50FBC3957")]
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

        private string _database;
        private string _filePath;
        private string _name;
        private long _ordinal;
        private List<string> _subsets;
        private string _text;
        private string _timeout;
        private string _warning;

        /// <summary>
        /// When the FilePath is set the Name and Ordinal fields are set from the path and the text
        /// of the file is automatically written into Text which automatically refreshes all the
        /// other fields as well
        /// </summary>
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

        /// <summary>
        /// When the Text field is set, the Variables, Subsets, Database, Timeout, and Warning fields
        /// are automatically refreshed
        /// </summary>
        public string Text
        {
            set
            {
                _text = value;
                RefreshMetadata();
            }
        }

        public IEnumerable<string> Variables { get; private set; }

        public string GetDatabase(Dictionary<string, string> variableMapping = null)
        {
            if (_database == null)
                return null;

            if (variableMapping != null)
                return variableMapping[_database.TrimStart('<').TrimEnd('>')] ?? _database;

            return _database;
        }

        public IEnumerable<string> GetSubsets(Dictionary<string, string> variableMapping = null)
        {
            if (_subsets == null)
                return new List<string>();

            if (variableMapping != null)
                return _subsets.Select(s => variableMapping[s.TrimStart('<').TrimEnd('>')] ?? s);

            return _subsets;
        }

        /// <summary>
        /// Returns the text of the script. If a variable mapping is passed in, any variables in the
        /// text will be replaced by their mapped value
        /// </summary>
        /// <param name="variableMapping"></param>
        /// <returns></returns>
        public string GetText(Dictionary<string, string> variableMapping = null)
        {
            if (_text == null)
                return null;

            StringBuilder returnText = new StringBuilder(_text);
            if (variableMapping != null)
            {
                var missing = Variables.Except(variableMapping.Keys).ToList();
                if (missing.Any())
                    throw new ArgumentException($"Missing variables: {missing.Aggregate("", (c, n) => c + n + ", ").TrimEnd(' ', ',')}");

                foreach (var v in variableMapping)
                {
                    returnText.Replace($"<<{v.Key}>>", v.Value);
                }
            }

            return returnText.ToString();
        }

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

        public string GetWarning(Dictionary<string, string> variableMapping = null)
        {
            if (_warning == null)
                return null;

            if (variableMapping != null)
                return variableMapping[_warning.TrimStart('<').TrimEnd('>')] ?? _warning;

            return _warning;
        }

        public void RereadFile()
        {
            Text = File.ReadAllText(_filePath);
        }

        private void RefreshMetadata()
        {
            Variables = ScriptUtility.LoadMultiTag(_text, @"<<(\w+?)>>").Distinct();

            _subsets = ScriptUtility.LoadMultiTag(_text, @"{{Subset=([\w<>]+?)}}").ToList();

            _timeout = ScriptUtility.LoadTag(_text, @"{{Timeout=([\w<>]+?)}}") ?? "6000";

            _warning = ScriptUtility.LoadTag(_text, @"{{Warning=(.+?)}}");

            _database = ScriptUtility.LoadTag(_text, @"{{Database=(.+?)}}");
        }
    }
}