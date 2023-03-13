using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace GetCircuitInfosByPos.Models
{
    public class Circuit
    {
        public string id { get; set; }
        public string Modele { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; } 
        public string DeviceID { get; set; }
        public int TypePrestationId { get; set; }
        public int TypeMaterielId { get; set; }
        public int DelegataireId { get; set; }
        [JsonProperty("location")]
        public Point Location { get; set; }

    }

  
}
