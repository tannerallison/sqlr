// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - IProcessStep.cs
// <Author></Author>
// <CreatedDate>2016-09-27</CreatedDate>
// <LastEditDate>2016-09-27</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    public interface IProcessStep
    {
        string FilePath { get; set; }

        string Name { get; }

        long Ordinal { get; }

        void HasChanges();
    }
}