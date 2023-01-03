namespace RoundReports
{
    public interface IReportStat
    {
        public string Title { get; }
        public int Order { get; }
        public void Setup();
        public void Cleanup();
    }
}
