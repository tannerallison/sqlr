using System.Collections.Generic;
using System.Linq;

namespace SQLr
{
    public class ConversionProject
    {
        /// <summary>
        /// Scripts within the last directory take precedence over scripts in the first directory
        /// </summary>
        private List<ScriptDirectory> ScriptDirectories { get; }

        public Dictionary<string, string> VariableValues { get; }

        private List<Script> GetScripts()
        {
            var directories = ScriptDirectories.ToList();
            directories.Reverse();

            HashSet<Script> scripts = new HashSet<Script>();
            foreach (ScriptDirectory directory in directories)
            {
                foreach (Script script in directory.Scripts)
                {
                    scripts.Add(script);
                }
            }

            return scripts.OrderBy(b => b.Ordinal).ToList();
        }
    }
}