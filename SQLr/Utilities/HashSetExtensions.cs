// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - HashSetExtensions.cs
// <Author></Author>
// <CreatedDate>2016-10-01</CreatedDate>
// <LastEditDate>2016-10-01</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr.Utilities
{
    #region using

    using System.Collections.Generic;

    #endregion

    public static class HashSetExtensions
    {
        public static void AddOrReplace<T>(this HashSet<T> hashSet, T item)
        {
            if (hashSet.Add(item))
                return;

            hashSet.Remove(item);
            hashSet.Add(item);
        }
    }
}