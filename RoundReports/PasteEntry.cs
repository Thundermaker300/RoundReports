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

    public class PasteResponse
    {
        public string id { get; set; }
        public string link { get; set; }
        public bool success { get; set; }
    }

    public class DiscordHook
    {
        public string username { get; set; }
        public string content { get; set; }
    }
}
