using Newtonsoft.Json;


namespace CosmosFunction.Models
{
    public class Features
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";

        [JsonProperty("geometry")]
        public Geometries geometry { get; set; }
    
    }
}
