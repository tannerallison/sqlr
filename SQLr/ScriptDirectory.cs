using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SQLr
{
	public class ScriptDirectory
	{
		#region  Constants, Statics & Fields

		private readonly string _directory;

		private readonly List<Script> _directoryScripts;
		private readonly FileSystemWatcher _watcher;

		#endregion

		#region Constructors & Destructors

		public ScriptDirectory(string directory, bool includeSubDirectories)
		{
			_directory = directory;
			_directoryScripts = new List<Script>();
			Variables = new HashSet<KeyValuePair<string, Script>>();
			Subsets = new HashSet<KeyValuePair<string, Script>>();

			ScanDirectory(includeSubDirectories);

			_watcher = new FileSystemWatcher(_directory)
			{
				EnableRaisingEvents = true,
				Filter = "*.sql",
				IncludeSubdirectories = includeSubDirectories,
				NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName
			};

			_watcher.Created += ScriptCreated;
			_watcher.Changed += ScriptChanged;
			_watcher.Renamed += ScriptRenamed;
			_watcher.Deleted += ScriptDeleted;
		}

		~ScriptDirectory()
		{
			_watcher.Created -= ScriptCreated;
			_watcher.Changed -= ScriptChanged;
			_watcher.Renamed -= ScriptRenamed;
			_watcher.Deleted -= ScriptDeleted;
		}

		#endregion

		#region Properties

		public List<Script> Scripts
		{
			get { return _directoryScripts.ToList(); }
		}

		public HashSet<KeyValuePair<string, Script>> Subsets { get; }

		public HashSet<KeyValuePair<string, Script>> Variables { get; }

		#endregion

		#region Methods

		public void AddInstance(Dictionary<string, HashSet<Script>> collection, string key, Script value)
		{
			if (!collection.ContainsKey(key))
				collection[key] = new HashSet<Script>();

			collection[key].Add(value);
		}

		public void UpdateVarsAndSubsets(Script s)
		{
			RemoveFromVarsAndSubs(s);
			foreach (var variable in s.Variables)
			{
				Variables.Add(new KeyValuePair<string, Script>(variable, s));
			}
			foreach (var subset in s.Subsets)
			{
				Subsets.Add(new KeyValuePair<string, Script>(subset, s));
			}
		}

		private Script GetScript(string path)
		{
			return _directoryScripts.FirstOrDefault(v => v.FilePath == path);
		}

		private void RemoveFromVarsAndSubs(Script s)
		{
			Variables.RemoveWhere(c => c.Value.Equals(s));
			Subsets.RemoveWhere(c => c.Value.Equals(s));
		}

		private void ScanDirectory(bool includeSubDirectories)
		{
			var files =
				Directory.GetFiles(_directory, "*.sql",
					includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
					.Where(v => Constants.ScriptRegex.Match(v.Substring(v.LastIndexOf('\\') + 1)).Success);

			foreach (var file in files)
			{
				var script = new Script(file);
				_directoryScripts.Add(script);
				UpdateVarsAndSubsets(script);
			}
		}

		private void ScriptChanged(object sender, FileSystemEventArgs e)
		{
			var script = GetScript(e.FullPath);
			script.RefreshMetadata();
			UpdateVarsAndSubsets(script);
		}

		private void ScriptCreated(object sender, FileSystemEventArgs e)
		{
			if (Constants.ScriptRegex.IsMatch(e.Name))
			{
				var script = new Script(e.FullPath);

				_directoryScripts.Add(script);
				UpdateVarsAndSubsets(script);
			}
		}

		private void ScriptDeleted(object sender, FileSystemEventArgs e)
		{
			var script = GetScript(e.FullPath);

			if (script != null)
			{
				_directoryScripts.Remove(script);
				RemoveFromVarsAndSubs(script);
			}
		}

		private void ScriptRenamed(object sender, RenamedEventArgs e)
		{
			var script = GetScript(e.OldFullPath);

			if (script == null)
				return;

			_directoryScripts.Remove(script);
			RemoveFromVarsAndSubs(script);

			var newScript = new Script(e.FullPath);
			_directoryScripts.Add(newScript);
			UpdateVarsAndSubsets(script);
		}

		#endregion
	}
}