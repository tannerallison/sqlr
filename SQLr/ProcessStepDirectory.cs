// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - ProcessStepDirectory.cs
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

    #endregion

    public class ProcessStepDirectory
    {
        private readonly string directory;
        private readonly Dictionary<string, IProcessStep> directoryProcessSteps;
        private readonly HashSet<IProcessStep> filteredSteps = new HashSet<IProcessStep>(new StepComparer());
        private readonly FileSystemWatcher watcher;

        public ProcessStepDirectory(string directory, string searchPattern, bool includeSubDirectories)
        {
            this.directory = directory;

            directoryProcessSteps = ScanDirectory(directory, searchPattern, includeSubDirectories);

            watcher = InitializeWatcher(searchPattern, includeSubDirectories);

            IsDirty = true;
        }

        ~ProcessStepDirectory()
        {
            watcher.Created -= FileCreated;
            watcher.Changed -= FileChanged;
            watcher.Renamed -= FileRenamed;
            watcher.Deleted -= FileDeleted;
        }

        /// <summary>
        ///     Are there changes to the scripts or files since the last time the Script property has
        ///     been pulled?
        /// </summary>
        public bool IsDirty { get; internal set; }

        /// <summary>
        ///     When this property is read, the IsDirty flag is switched back to false
        /// </summary>
        public List<IProcessStep> Steps
        {
            get
            {
                if (IsDirty)
                {
                    filteredSteps.Clear();

                    foreach (var step in directoryProcessSteps.OrderBy(v => v.Key, new FilePathComparer()))
                    {
                        if (filteredSteps.Contains(step.Value))
                            continue;

                        filteredSteps.Add(step.Value);
                    }
                }

                IsDirty = false;
                return filteredSteps.OrderBy(v => v.Ordinal).ToList();
            }
        }

        private static Dictionary<string, IProcessStep> ScanDirectory(
            string directory,
            string searchPattern,
            bool includeSubDirectories)
        {
            var files =
                Directory.GetFiles(
                        directory,
                        searchPattern,
                        includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(v => Constants.ScriptRegex.Match(v.Substring(v.LastIndexOf('\\') + 1)).Success);

            var scannedScripts = new Dictionary<string, IProcessStep>();
            foreach (var file in files)
            {
                var script = new Script(file);
                scannedScripts.Add(file, script);
            }

            return scannedScripts;
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            var step = GetStep(e.FullPath);
            step.HasChanges();
            IsDirty = true;
        }

        private void FileCreated(object sender, FileSystemEventArgs e)
        {
            if (Constants.ScriptRegex.IsMatch(e.Name))
            {
                var step = new Script(e.FullPath);
                directoryProcessSteps.Add(e.FullPath, step);
                IsDirty = true;
            }
        }

        private void FileDeleted(object sender, FileSystemEventArgs e)
        {
            var step = GetStep(e.FullPath);

            if (step != null)
            {
                directoryProcessSteps.Remove(e.FullPath);
                IsDirty = true;
            }
        }

        private void FileRenamed(object sender, RenamedEventArgs e)
        {
            directoryProcessSteps.Remove(e.OldFullPath);

            var newStep = new Script(e.FullPath);
            directoryProcessSteps.Add(e.FullPath, newStep);
            IsDirty = true;
        }

        private IProcessStep GetStep(string path)
        {
            if (directoryProcessSteps.ContainsKey(path))
                return directoryProcessSteps[path];
            return null;
        }

        private FileSystemWatcher InitializeWatcher(string filter, bool includeSubDirectories)
        {
            var fileWatcher = new FileSystemWatcher(directory)
                                  {
                                      EnableRaisingEvents = true,
                                      Filter = filter,
                                      IncludeSubdirectories = includeSubDirectories,
                                      NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName
                                  };

            fileWatcher.Created += FileCreated;
            fileWatcher.Changed += FileChanged;
            fileWatcher.Renamed += FileRenamed;
            fileWatcher.Deleted += FileDeleted;

            return fileWatcher;
        }

        /// <summary>
        ///     This will sort by depth of filepath first, then by alphabetically
        /// </summary>
        private class FilePathComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y)
                    return 0;

                var xPath = x.Split('\\');
                var yPath = y.Split('\\');

                // If the new path is deeper than the existing path, ignore it
                if (xPath.Length < yPath.Length)
                    return -1;

                // If the existing path is deeper than the new path, replace it
                if (xPath.Length > yPath.Length)
                    return 1;

                // Drill down to the first point where the paths diverge
                var i = 0;
                while (xPath[i] == yPath[i])
                    i++;

                // If the new path comes before the existing alphabetically, replace it
                return string.Compare(xPath[i], yPath[i], StringComparison.Ordinal);
            }
        }
    }
}