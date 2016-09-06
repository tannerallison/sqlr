using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SQLr
{
	public class Script
	{
		#region  Constants, Statics & Fields
		#endregion

		#region Constructors & Destructors

		public Script(string scriptFilePath)
		{
			FilePath = scriptFilePath;

			if (!Constants.ScriptRegex.IsMatch(scriptFilePath))
				throw new ArgumentException("The file name given is not valid in the conversion application.");

			Name = Path.GetFileNameWithoutExtension(FilePath);

			RefreshMetadata();
		}

		#endregion

		#region Properties

		public string Name { get; }
		public string Database { get; private set; }

		public string FilePath { get; }

		public long Ordinal { get; private set; }
		public IEnumerable<string> Subsets { get; private set; }

		public string Text { get; private set; }
		public int Timeout { get; private set; }

		public IEnumerable<string> Variables { get; private set; }

		public string WarningMessage { get; private set; }

		#endregion

		#region Methods

		public void RefreshMetadata()
		{
			// Set the Script Text
			Text = File.ReadAllText(FilePath);

			// Set Ordinal
			long ordinal;
			if (long.TryParse(Constants.ScriptRegex.Match(FilePath).Groups[1].Value, out ordinal))
				Ordinal = ordinal;
			else
				throw new Exception("The ordinal did not parse correctly");

			// Identify Variables
			Variables = LoadMultiTag(Text, "<<([a-z0-9_]+?)>>").Distinct();

			// Identify Subsets
			Subsets = LoadMultiTag(Text, "\\{\\{subset=([a-z0-9_]+?)}}");

			// Identify a Timeout
			int timeoutVal;
			Timeout = int.TryParse(LoadTag(Text, "\\{\\{Timeout=([0-9]+?)}}"), out timeoutVal)
				? timeoutVal
				: 6000;

			// Identify a Warning Message
			WarningMessage = LoadTag(Text, "\\{\\{Warn=(.+?)}}");
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

		private void ScriptWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (FilePath != e.FullPath)
				return;

			HasChanges = true;
		}


		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof(Script)) return false;
			Script other = obj as Script;
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Name, Name);
		}
		
		#endregion
	}
}