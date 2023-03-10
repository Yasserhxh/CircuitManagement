using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace TrackingSys
{
    public static class TrackingCF
    {

        [FunctionName("TrackingCF")]
        public static async Task Run(
            [CosmosDBTrigger(
            databaseName: "tracking-db",
            collectionName: "tracking-live",
            ConnectionStringSetting = "CosmosDbConnectionString",
            LeaseCollectionName = "materializedViewLease",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [SignalR(HubName = "trackingdata")] IAsyncCollector<SignalRMessage> signalRMessages,
            [CosmosDB(
                databaseName: "tracking-db",
                collectionName: "trackingByDevice",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<Document> output, ILogger log)

        {
            foreach (var v in input)
            {
                await signalRMessages.AddAsync(
                   new SignalRMessage
                   {
                       Target = "newPosition",
                       Arguments = new[] { v }
                   });
                await output.AddAsync(v);

            }
        }
    }

}
