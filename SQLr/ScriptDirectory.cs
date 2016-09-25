using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SQLr
{
    public class ScriptDirectory
    {
        public ScriptDirectory(string directory, bool includeSubDirectories)
        {
            _directory = directory;
            _directoryScripts = new List<Script>();

            ScanDirectory(includeSubDirectories);

            _watcher = InitializeWatcher(includeSubDirectories);
        }

        ~ScriptDirectory()
        {
            _watcher.Created -= ScriptCreated;
            _watcher.Changed -= ScriptChanged;
            _watcher.Renamed -= ScriptRenamed;
            _watcher.Deleted -= ScriptDeleted;
        }

        private readonly string _directory;
        private readonly List<Script> _directoryScripts;
        private readonly FileSystemWatcher _watcher;

        public List<Script> Scripts
        {
            get
            {
                IsDirty = false;
                return _directoryScripts.ToList();
            }
        }

        public bool IsDirty { get; internal set; }

        public void AddInstance(Dictionary<string, HashSet<Script>> collection, string key, Script value)
        {
            if (!collection.ContainsKey(key))
                collection[key] = new HashSet<Script>();

            collection[key].Add(value);
        }

        private Script GetScript(string path)
        {
            return _directoryScripts.FirstOrDefault(v => v.FilePath == path);
        }

        private FileSystemWatcher InitializeWatcher(bool includeSubDirectories)
        {
            var watcher = new FileSystemWatcher(_directory)
            {
                EnableRaisingEvents = true,
                Filter = "*.sql",
                IncludeSubdirectories = includeSubDirectories,
                NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            watcher.Created += ScriptCreated;
            watcher.Changed += ScriptChanged;
            watcher.Renamed += ScriptRenamed;
            watcher.Deleted += ScriptDeleted;

            return watcher;
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

                var existingScript = _directoryScripts.FirstOrDefault(v => v.Name == script.Name && v.Ordinal == script.Ordinal);

                if (existingScript != null)
                {
                    var existingPath = Path.GetDirectoryName(existingScript.FilePath).Split('\\');
                    var newPath = Path.GetDirectoryName(script.FilePath).Split('\\');

                    if (newPath.Length < existingPath.Length)
                    {
                        _directoryScripts.Remove(existingScript);
                        _directoryScripts.Add(script);
                        continue;
                    }
                    else if (newPath.Length > existingPath.Length)
                    {
                        continue;
                    }

                    int i = 0;
                    while (existingPath[i] == newPath[i])
                        i++;

                    if (newPath[i].CompareTo(existingPath[i]) < 0)
                    {
                        _directoryScripts.Remove(existingScript);
                        _directoryScripts.Add(script);
                        continue;
                    }
                    else continue;
                }

                _directoryScripts.Add(script);
            }
        }

        private void ScriptChanged(object sender, FileSystemEventArgs e)
        {
            var script = GetScript(e.FullPath);
            script.RereadFile();
            IsDirty = true;
        }

        private void ScriptCreated(object sender, FileSystemEventArgs e)
        {
            if (Constants.ScriptRegex.IsMatch(e.Name))
            {
                var script = new Script(e.FullPath);
                _directoryScripts.Add(script);
                IsDirty = true;
            }
        }

        private void ScriptDeleted(object sender, FileSystemEventArgs e)
        {
            var script = GetScript(e.FullPath);

            if (script != null)
            {
                _directoryScripts.Remove(script);
                IsDirty = true;
            }
        }

        private void ScriptRenamed(object sender, RenamedEventArgs e)
        {
            var script = GetScript(e.OldFullPath);

            if (script == null)
                return;

            _directoryScripts.Remove(script);

            var newScript = new Script(e.FullPath);
            _directoryScripts.Add(newScript);
            IsDirty = true;
        }
    }
}