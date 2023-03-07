using Newtonsoft.Json;
using System.Collections.Generic;


namespace CosmosFunction.Models
{
    public class Circuit
    {
        public string id { get; set; }
        public int CircuitId { get; set; }
        public int ZoneId { get; set; }
        public int SecteurId { get; set; } 
        public string CircuitNum { get; set; }
        public int PrestationId { get; set; }
        public int DelegataireId { get; set; }
        public GeoZone GeoZone { get; set; }

    }

  
}
