using Newtonsoft.Json;
using System.Collections.Generic;


namespace CosmosFunction.Models
{
    public class GeoZone
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "FeatureCollection";

        [JsonProperty("features")]
        public List<Features> Features { get; set; }


    }
}
