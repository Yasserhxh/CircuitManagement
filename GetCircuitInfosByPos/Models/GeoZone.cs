using Newtonsoft.Json;
using System.Collections.Generic;


namespace GetCircuitInfosByPos.Models
{
    public class GeoZone
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Polygon";

        [JsonProperty("coordinates")]
        public List<double> Coordinates { get; set; }


    }
}
