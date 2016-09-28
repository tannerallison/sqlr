// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - ConversionProject.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    #region using

    using System.Collections.Generic;
    using System.Linq;

    #endregion

    public class ConversionProject
    {
        private readonly HashSet<Script> scripts;

        public ConversionProject()
        {
            scripts = new HashSet<Script>(new StepComparer());
            ScriptDirectories = new List<ProcessStepDirectory>();
        }

        /// <summary>
        ///     An ordered list of directories that contain scripts. Steps with the same file name in
        ///     later ScriptDirectories will overwrite prior scripts.
        /// </summary>
        public List<ProcessStepDirectory> ScriptDirectories { get; }

        /// <summary>
        ///     Cycles through all the script directories that have changes and retrieves the changes.
        /// </summary>
        /// <returns>
        ///     The list of scripts within the ScriptDirectories.
        /// </returns>
        public List<Script> GetScripts()
        {
            if ((scripts.Count == 0) || ScriptDirectories.Any(v => v.IsDirty))
            {
                foreach (var t in ScriptDirectories)
                {
                    foreach (var s in t.Steps.OfType<Script>())
                    {
                        if (scripts.Add(s))
                            continue;

                        scripts.Remove(s);
                        scripts.Add(s);
                    }
                }
            }

            return scripts.OrderBy(v => v.Ordinal).ToList();
        }
    }
}