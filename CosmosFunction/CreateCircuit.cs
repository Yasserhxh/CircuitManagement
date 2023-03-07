using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CosmosFunction.Models;
using System;
using Microsoft.Azure.Cosmos.Spatial;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using System.Data.SqlClient;
using System.Text;

namespace CosmosFunction
{
    public static class CreateCircuit
    {
        [FunctionName("CreateCircuit")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "CircuitDB", collectionName: "RefCircuit", ConnectionStringSetting = "CosmosDbConnectionString")] 
            IAsyncCollector<Circuit> documents, ILogger log)                     
        {

            string _circuitId = req.Query["CircuitId"];
            string _zoneId = req.Query["ZoneId"];
            string _secteurId = req.Query["SecteurId"];
            string _circuitNum = req.Query["CircuitNum"];
            string _prestationId = req.Query["PrestationId"];
            string _delegataireId = req.Query["DelegataireId"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var _coordinates = /*data?.features[0].geometry*/data?.geometry.coordinates.ToString();
            dynamic _geozone = JsonConvert.DeserializeObject(_coordinates);
            var geo1 = _geozone[0];
            int circuitId = int.Parse(_circuitId);
            int zoneId = int.Parse(_zoneId);
            int secteurId = int.Parse(_secteurId);
            string circuitNum = _circuitNum;
            int prestationId = int.Parse(_prestationId);
            int delegataireId = int.Parse(_delegataireId);         


            var newRatingGuid = Guid.NewGuid().ToString();

            if (zoneId == 0 || prestationId == 0 || circuitId == 0 || circuitNum == null)
            {
                return new BadRequestObjectResult("Please pass a ZoneId, PrestationId, CircuitId and CircuitNum in the request body");
            }
            else
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
            }

            Circuit circuit = new()
            {
                id = newRatingGuid,
                CircuitId = circuitId,
                ZoneId = zoneId,
                SecteurId = secteurId,
                CircuitNum = circuitNum,
                PrestationId = prestationId,
                DelegataireId = delegataireId
                             
            };

            var Coordinates = JsonConvert.DeserializeObject<List<List<double>>>(_coordinates);
            var listcoords = new List<List<List<double>>>
            {
                Coordinates
            };
            circuit.GeoZone = new GeoZone
            {
                Type = "FeatureCollection",
            
                Features = new List<Features>
                {
                    new Features
                    {
                        Type = "Feature",
                        geometry = new Geometries
                        {
                            Type = "Polygon",
                            Coordinates = listcoords //JsonConvert.DeserializeObject<List<List<List<double>>>>(_coordinates)
                        }
                    }
                }
            };
            await documents.AddAsync(circuit);

            try
            {
                using SqlConnection connection = new(Environment.GetEnvironmentVariable("SQLDbConnectionString"));
                connection.Open();
                if (!String.IsNullOrEmpty(circuit.id))
                {
                    /** Debut Geo Loc to WKT **/
                    StringBuilder builder = new();
                    builder.Append("LINESTRING(");
                    foreach (var item in listcoords[0])
                    {
                        for (int i = 0; i < item.Count; i += 2)
                        {

                            builder.Append(item[i].ToString().Replace(",", ".") + " " + item[i + 1].ToString().Replace(",", "."));
                            builder.Append(',');

                        }
                    }
                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(')');
                    var WKT = builder.ToString();
                    /** Fin Geo Loc to WKT **/
                    var query = $"INSERT INTO [Circuit] (Circuit_ZoneId,Circuit_DelegataireId,Circuit_Numero,Circuit_DelimitationGoe,Circuit_PrestationId,Circuit_IsActive,Circuit_DateCreation,Circuit_Identifier) VALUES('{circuit.ZoneId}', '{circuit.DelegataireId}' , '{circuit.CircuitNum}','{WKT}','{circuit.PrestationId}','{true}','{DateTime.Now}','{circuit.id}')";
                    SqlCommand command = new(query, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }

            return new OkObjectResult(circuit);

        }
    }
}
