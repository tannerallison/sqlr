// --------------------------------------------------------------------------------------------------------------------
// SQLr - SQLr - ConversionProject.cs
// <Author></Author>
// <CreatedDate>2016-09-23</CreatedDate>
// <LastEditDate>2016-10-01</LastEditDate>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SQLr
{
    #region using

    using System.Collections.Generic;
    using System.Linq;
    using SQLr.ProcessStep;
    using SQLr.Utilities;

    #endregion

    public class ConversionProject
    {
        private readonly HashSet<IProcessStep> steps;

        public ConversionProject()
        {
            steps = new HashSet<IProcessStep>(new StepComparer());
            StepDirectories = new List<ProcessStepDirectory>();
        }

        /// <summary>
        ///     An ordered list of directories that contain steps. Steps with the same file name in
        ///     later StepDirectories will overwrite prior steps.
        /// </summary>
        public List<ProcessStepDirectory> StepDirectories { get; }

        /// <summary>
        ///     Cycles through all the script directories that have changes and retrieves the changes.
        /// </summary>
        /// <returns>
        ///     The list of steps within the StepDirectories.
        /// </returns>
        public List<IProcessStep> GetSteps()
        {
            if ((steps.Count == 0) || StepDirectories.Any(v => v.IsDirty))
            {
                for (var i = 0; i < StepDirectories.Count; i++)
                {
                    var t = StepDirectories[i];
                    t.Steps.ForEach(v => steps.AddOrReplace(v));
                }
            }

            return steps.OrderBy(v => v.Ordinal).ToList();
        }
    }
}