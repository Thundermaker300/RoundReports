using Newtonsoft.Json;
using System.Collections.Generic;

namespace RoundReports
{
    public class GraphData
    {
        [JsonProperty("labels")]
        public string[] Labels { get; set; }

        [JsonProperty("datasets")]
        public List<GraphDataSet> DataSets { get; set; }
    }

    public class GraphDataSet
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("data")]
        public float[] Data { get; set; }
    }

    public class GraphSettings
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public GraphData Data { get; set; }
    }
}
