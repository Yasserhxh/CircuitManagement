using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CosmosFunction.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Policy;

namespace CosmosFunction
{
    public static class CreateCircuitSQL
    {
        [FunctionName("CreateCircuitSQL")]
        public static void Run([CosmosDBTrigger(
            databaseName: "CircuitDB",
            collectionName: "RefCircuit",
            ConnectionStringSetting = "CosmosDbConnectionString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [CosmosDB(databaseName: "CircuitDB",
            collectionName: "RefCircuitByPresId",
            ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<Document> output, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                dynamic data = JsonConvert.DeserializeObject(input.FirstOrDefault().ToString());
                var _coordinates = /*data?.features[0].geometry*/data?.GeoZone.features[0].geometry.coordinates.ToString();
                var listcoords = JsonConvert.DeserializeObject<List<List<List<double>>>>(_coordinates);

                Circuit circuit = new()
                {
                    id = data?.id,
                   // CircuitId = data?.CircuitId,
                    ZoneId = data?.ZoneId,
                    SecteurId = data?.SecteurId,
                    CircuitNum = data?.CircuitNum,
                    PrestationId = data?.PrestationId,
                    DelegataireId = data?.DelegataireId

                };
                try
                {
                    using SqlConnection connection = new(Environment.GetEnvironmentVariable("SQLDbConnectionString"));
                    connection.Open();
                    var circuitSQL = $"SELECT Circuit_Identifier FROM [Circuit] WHERE Circuit_Identifier = '{circuit.id}'";
                    SqlCommand command = new(circuitSQL, connection);
                    //command.ExecuteNonQuery();
                    string Id = (string)(command.ExecuteScalar());
                    if (!string.IsNullOrEmpty(Id))
                    {
                        log.LogInformation(Id);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(circuit.id))
                        {
                            /** Debut Geo Loc to WKT **/
                            StringBuilder builder = new();
                            builder.Append("Polygon(");
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
                            SqlCommand Insertcommand = new(query, connection);
                            Insertcommand.ExecuteNonQuery();
                        }
                    }
                 
                  
                }
                catch (Exception e)
                {
                    log.LogError(e.ToString());
                }
                
            }
        }
    }
}
