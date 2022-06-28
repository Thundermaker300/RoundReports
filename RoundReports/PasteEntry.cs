using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class PasteEntry
    {
        public string description { get; set; }
        public List<PasteSection> sections { get; set; }
    }

    public class PasteSection
    {
        public string name { get; set; }
        public string syntax { get; set; }
        public string contents { get; set; }
    }
}
