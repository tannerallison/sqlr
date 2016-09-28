// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - ScriptDirectory.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class ScriptDirectory
    {
        private readonly string directory;
        private readonly Dictionary<string, Script> directoryScripts;
        private readonly FileSystemWatcher watcher;

        public ScriptDirectory(string directory, bool includeSubDirectories)
        {
            this.directory = directory;

            directoryScripts = ScanDirectory(directory, includeSubDirectories);

            watcher = InitializeWatcher(includeSubDirectories);
        }

        ~ScriptDirectory()
        {
            watcher.Created -= ScriptCreated;
            watcher.Changed -= ScriptChanged;
            watcher.Renamed -= ScriptRenamed;
            watcher.Deleted -= ScriptDeleted;
        }

        /// <summary>
        ///     Are there changes to the scripts or files since the last time the Script property has
        ///     been pulled?
        /// </summary>
        public bool IsDirty { get; internal set; }

        /// <summary>
        ///     When this property is read, the IsDirty flag is switched back to false
        /// </summary>
        public List<Script> Scripts
        {
            get
            {
                IsDirty = false;
                return directoryScripts.Values.OrderBy(v => v.Ordinal).ToList();
            }
        }

        private static void HandleDuplicateFileNames(
            ref Dictionary<string, Script> scannedScripts,
            string key,
            Script newScript)
        {
            var existingScript = scannedScripts[key];
            var existingPath = Path.GetDirectoryName(existingScript.FilePath).Split('\\');
            var newPath = Path.GetDirectoryName(newScript.FilePath).Split('\\');

            // If the new path is deeper than the existing path, ignore it
            if (newPath.Length > existingPath.Length)
                return;

            // If the existing path is deeper than the new path, replace it
            if (newPath.Length < existingPath.Length)
            {
                scannedScripts[key] = newScript;
                return;
            }

            // Drill down to the first point where the paths diverge
            var i = 0;
            while (existingPath[i] == newPath[i])
                i++;

            // If the new path comes before the existing alphabetically, replace it
            if (newPath[i].CompareTo(existingPath[i]) < 0)
                scannedScripts[key] = newScript;
        }

        private static Dictionary<string, Script> ScanDirectory(string directory, bool includeSubDirectories)
        {
            var files =
                Directory.GetFiles(
                        directory,
                        "*.sql",
                        includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(v => Constants.ScriptRegex.Match(v.Substring(v.LastIndexOf('\\') + 1)).Success);

            var scannedScripts = new Dictionary<string, Script>();
            foreach (var file in files)
            {
                var script = new Script(file);

                var key = Path.GetFileName(file);
                if (includeSubDirectories && scannedScripts.ContainsKey(key))
                    HandleDuplicateFileNames(ref scannedScripts, key, script);
                else
                    scannedScripts.Add(key, script);
            }

            return scannedScripts.ToDictionary(k => k.Value.FilePath, v => v.Value);
        }

        private Script GetScript(string path)
        {
            if (directoryScripts.ContainsKey(path))
                return directoryScripts[path];
            return null;
        }

        private FileSystemWatcher InitializeWatcher(bool includeSubDirectories)
        {
            var watcher = new FileSystemWatcher(directory)
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
                directoryScripts.Add(e.FullPath, script);
                IsDirty = true;
            }
        }

        private void ScriptDeleted(object sender, FileSystemEventArgs e)
        {
            var script = GetScript(e.FullPath);

            if (script != null)
            {
                directoryScripts.Remove(e.FullPath);
                IsDirty = true;
            }
        }

        private void ScriptRenamed(object sender, RenamedEventArgs e)
        {
            directoryScripts.Remove(e.OldFullPath);

            var newScript = new Script(e.FullPath);
            directoryScripts.Add(e.FullPath, newScript);
            IsDirty = true;
        }
    }
}