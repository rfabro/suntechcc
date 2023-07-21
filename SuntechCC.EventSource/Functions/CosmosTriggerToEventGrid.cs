using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using SuntechCC.EventSource.Models;

namespace SuntechCC.EventSource.Functions;

public static class CosmosTriggerToEventGrid
{
    /// <summary>
    /// Function that will listen to Cosmosdb changes and pushes the document into Event Grid
    /// </summary>
    /// <param name="trigger">CosmosDb change feed</param>
    /// <param name="eventCollector">Event Grid</param>
    [FunctionName("CosmosTrigger")]
    public static async Task RunAsync(
            // cosmosdb input trigger
            [CosmosDBTrigger(
            databaseName: "suntechdb",
            containerName: "users",
            Connection = "CosmosDbConnectionString",
            CreateLeaseContainerIfNotExists  = true,
            LeaseContainerName = "userslease")]
            IReadOnlyList<UserModel> users,
            ILogger log,
            // event grid output binding
            [EventGrid(TopicEndpointUri = "EventGridEndpoint", TopicKeySetting = "EventGridKey")]
            IAsyncCollector<EventGridEvent>  eventCollector)
    {
        if (users != null && users.Count > 0)
        {
            log.LogInformation("User modified count: " + users.Count);
            foreach (var user in users)
            {
                // serialize user into JSON
                var userDetailsRequestStr = JsonSerializer.Serialize(user);
                log.LogInformation("User: " + userDetailsRequestStr);

                // dispatch to event grid
                var eventGridEvent = new EventGridEvent(userDetailsRequestStr, "IncomingRequest", "IncomingRequest", "1.0.0");
                await eventCollector.AddAsync(eventGridEvent);
            }
        }
    }
}