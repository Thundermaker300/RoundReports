using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public interface IReportStat
    {
        public string Title { get; }
        public int Order { get; }
    }
}
