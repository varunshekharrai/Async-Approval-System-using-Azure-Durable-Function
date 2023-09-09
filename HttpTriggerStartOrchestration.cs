using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsOrchestration;

/// <summary>
/// This is the HTTP trigger class for starting the orchestration.
/// </summary>
public static class HttpTriggerStartOrchestration
{
    /// <summary>
    /// This is the start HTTP trigger function that will start the <see cref="DurableFunctionsOrchestration"/> orchestration.
    /// </summary>
    /// <param name="req">The request object of the GET API call.</param>
    /// <param name="starter">The durable orchestration client.</param>
    /// <param name="log">The logger object.</param>
    /// <returns></returns>
    [FunctionName("start")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        // Function input comes from the request content.
        string instanceId = await starter.StartNewAsync("DurableFunctionsOrchestration", null);

        log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}