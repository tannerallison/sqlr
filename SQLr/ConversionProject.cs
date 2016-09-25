using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLr
{
    public class ConversionProject
    {
        public ConversionProject()
        {
            _scriptDirectories = new List<ScriptDirectory>();
        }

        private List<ScriptDirectory> _scriptDirectories;

        public List<ScriptDirectory> ScriptDirectories { get { return _scriptDirectories; } }

        private HashSet<Script> scripts = new HashSet<Script>(new ScriptComparer());

        public List<Script> GetScripts()
        {
            if (scripts == null || scripts.Count == 0 || _scriptDirectories.Any(v => v.IsDirty))
            {
                for (int i = _scriptDirectories.Count - 1; i >= 0; i--)
                {
                    foreach (var s in _scriptDirectories[i].Scripts)
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

        public class ScriptComparer : IEqualityComparer<Script>
        {
            public bool Equals(Script one, Script two)
            {
                return one.Ordinal == two.Ordinal && one.Name == two.Name;
            }

            public int GetHashCode(Script item)
            {
                return (item.Name + item.Ordinal.ToString()).GetHashCode();
            }
        }
    }
}