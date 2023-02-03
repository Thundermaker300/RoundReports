namespace RoundReports
{
    /// <summary>
    /// Defines a stat section to be shown on the round report.
    /// </summary>
    public interface IReportStat
    {
        /// <summary>
        /// Gets the title of the section.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the order of the section, relative to other sections.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Called when the stat is first created.
        /// </summary>
        public void Setup();

        /// <summary>
        /// Called when the stat is being cleaned up.
        /// </summary>
        public void Cleanup();

        /// <summary>
        /// Called when the round ends, before the stats are uploaded.
        /// </summary>
        public void FillOutFinal();
    }
}
