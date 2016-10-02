// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - IProcessStep.cs
// <Author></Author>
// <CreatedDate>2016-09-28</CreatedDate>
// <LastEditDate>2016-10-01</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr.ProcessStep
{
    public interface IProcessStep
    {
        string FilePath { get; set; }

        string Name { get; }

        long Ordinal { get; }

        void HasChanges();
    }
}