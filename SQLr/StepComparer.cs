namespace SQLr
{
    using System.Collections.Generic;

    public class StepComparer : IEqualityComparer<IProcessStep>
    {
        public bool Equals(IProcessStep one, IProcessStep two)
        {
            return (one.Ordinal == two.Ordinal) && (one.Name == two.Name);
        }

        public int GetHashCode(IProcessStep item) { return (item.Name + item.Ordinal).GetHashCode(); }
    }
}