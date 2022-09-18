using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class PasteEntry
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("expiration")]
        public long Expiration { get; set; }

        [JsonProperty("sections")]
        public List<PasteSection> Sections { get; set; }
    }

    public class PasteSection
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("syntax")]
        public string Syntax { get; set; }

        [JsonProperty("contents")]
        public string Contents { get; set; }
    }

    public class PasteResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
