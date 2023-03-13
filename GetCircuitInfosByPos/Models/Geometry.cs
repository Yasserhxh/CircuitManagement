using Newtonsoft.Json;
using System.Collections.Generic;

namespace GetCircuitInfosByPos.Models
{
    public class Geometries
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Polygon";

        [JsonProperty("coordinates")]
        public List<List<List<double>>> Coordinates { get; set; }
    }
}
