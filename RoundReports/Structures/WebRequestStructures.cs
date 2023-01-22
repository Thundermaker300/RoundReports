namespace RoundReports
{
#pragma warning disable SA1402
#pragma warning disable SA1649
#pragma warning disable SA1600
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Internal Pastee web request structure.
    /// </summary>
    public class PasteEntry
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("expiration")]
        public long Expiration { get; set; }

        [JsonProperty("sections")]
        public List<PasteSection> Sections { get; set; }
    }

    /// <summary>
    /// Internal Pastee section structure.
    /// </summary>
    public class PasteSection
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("syntax")]
        public string Syntax { get; set; }

        [JsonProperty("contents")]
        public string Contents { get; set; }
    }

    /// <summary>
    /// Internal response from Pastee.
    /// </summary>
    public class PasteResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
#pragma warning restore SA1402
#pragma warning restore SA1649
#pragma warning restore SA1600
}
