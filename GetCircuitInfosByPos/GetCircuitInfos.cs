using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using Microsoft.Azure.Cosmos.Spatial;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using System.Data.SqlClient;
using System.Text;
using GetCircuitInfosByPos.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Collections;
using System.ComponentModel;

namespace CosmosFunction
{
    public static class GetCircuitInfos
    {
        [FunctionName("GetCircuitInfos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route =null)] HttpRequest req,
            [CosmosDB(databaseName: "CircuitDB", collectionName: "RefCircuitByPresId",  ConnectionStringSetting = "CosmosDbConnectionString")]
            DocumentClient  documents, ILogger log)                     
        {

            //  string _date = req.Query["date"];
            string _prestationId = req.Query["PrestationId"]; // read _prestationId to get driver for from querystring


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var _coordinates = /*data?.features[0].geometry*/data?.geometry.coordinates.ToString();
            int prestationId = int.Parse(_prestationId);
           // DateTime? date = Convert.ToDateTime(_date);         



            if (prestationId == 0 )
            {
                return new BadRequestObjectResult("Please pass a PrestationId in the request body");
            }
            else
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
            }

            Circuit circuit = new()
            {
                PrestationId = prestationId                             
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
            Uri driverCollectionUri = UriFactory.CreateDocumentCollectionUri("CircuitDB","RefCircuitByPresId");
            //var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query
            IDocumentQuery<Circuit> Circuits = documents.CreateDocumentQuery<Circuit>(driverCollectionUri)
                .Where(circuit => circuit.PrestationId == prestationId)
                .AsDocumentQuery();
           /* Microsoft.Azure.Cosmos.Container container = client.GetDatabase("CircuitDB").GetContainer("RefCircuitByPresId");

            foreach (Circuit user in container.GetItemLinqQueryable<UserProfile>(allowSynchronousQueryExecution: true).Where(u => u.ProfileType == "Public" && u.Location.Distance(new Point(32.33, -4.66)) < 30000))
            {
                Console.WriteLine("\t" + user);
            }*/

            var circuitsForStore = new List<Circuit>();

            while (Circuits.HasMoreResults)
            {
                foreach (Circuit item in (await Circuits.ExecuteNextAsync()).Select(v => (Circuit)v))
                {
                    circuitsForStore.Add(item);
                }
            }

            return new OkObjectResult(circuitsForStore);

        }
    }
}
