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
using GetCircuitInfosByPos.Models;
using System.Linq;

namespace CosmosFunction
{
    public static class GetCircuitInfos
    {
        [FunctionName("GetCircuitInfos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route =null)] HttpRequest req,
            [CosmosDB(databaseName: "tracking-db", containerName: "trackingByPresId",  Connection = "CosmosDbConnectionString")]
            CosmosClient  client, ILogger log)                     
        {

            string _date = req.Query["date"];
            string _prestationId = req.Query["PrestationId"]; 

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            //var _coordinates = /*data?.features[0].geometry*/data?.geometry.coordinates.ToString();
            int prestationId = int.Parse(_prestationId);
            if (prestationId == 0)
            {
                return new BadRequestObjectResult("Please pass a PrestationId in the request body");
            }
            else
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
            }     
            var userPoint = new Point(-7.537067, 33.59421);           
            try
            {
                Container container = client.GetDatabase("tracking-db").GetContainer("trackingByPresId");

                /*** USING LINQ EXPRESSION ***/

                /*var circuits = container.GetItemLinqQueryable<Circuit>(allowSynchronousQueryExecution: true)
                    .Where(c => c.Location.Distance(userPoint) >= 10000 && c.Location.Distance(userPoint) < 10000)
                    .Where(c => c.TypePrestationId == prestationId && c.Time.Contains(_date));*/

                QueryDefinition queryDefinition = new QueryDefinition(
                    "SELECT * FROM trackingByPresId AS f WHERE ST_DISTANCE(f.location, {type: @type, coordinates:[@long,@lat]}) >= 0 AND ST_DISTANCE(f.location, {type: @type, coordinates:[@long,@lat]}) <= 10 AND f.TypePrestationId = @prestationId  AND CONTAINS(f.time,@date)")
                    .WithParameter("@long", userPoint.Position.Longitude)
                    .WithParameter("@lat", userPoint.Position.Latitude)
                    .WithParameter("@prestationId", prestationId)
                    .WithParameter("@date", _date)
                    .WithParameter("@type", "Point");
                using FeedIterator<Circuit> resultSet = container.GetItemQueryIterator<Circuit>(queryDefinition);
                List<Circuit> result = new();
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<Circuit> response = await resultSet.ReadNextAsync();
                    Circuit item = response.First();
                    result.Add(item);
                    log.LogInformation(item.id);
                }
                return new OkObjectResult(result.FirstOrDefault());

            }
            catch (Exception ex)
            {
                log.LogError(ex.Message.ToString());

            }
            return new OkResult();
        }
    }
}
