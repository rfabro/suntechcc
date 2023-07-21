// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridTrigger

using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace SuntechCC.API.Functions;

public static class EventGridTrigger
{
    [FunctionName("EventGridTrigger")]
    public static void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
    {
        log.LogInformation(eventGridEvent.Data.ToString());
        log.LogInformation($@"New Event Grid Event:
            - Id=[{eventGridEvent.Id}]
            - EventType=[{eventGridEvent.EventType}]
            - EventTime=[{eventGridEvent.EventTime}]
            - Subject=[{eventGridEvent.Subject}]
            - Topic=[{eventGridEvent.Topic}]");
    }
}