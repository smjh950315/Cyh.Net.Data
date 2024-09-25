#pragma warning disable
using Cyh;
using Cyh.Net;
using Cyh.Net.Data;
using System.Text.Json.Serialization;

namespace Cyh.Net.Data
{
    public class DataRange
    {
        [JsonPropertyName("begin")]
        public int Begin { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
