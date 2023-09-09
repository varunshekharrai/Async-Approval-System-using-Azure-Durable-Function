using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsOrchestration;

/// <summary>
/// This is the HTTP trigger class for approval.
/// </summary>
public static class HttpTriggerApproval
{
    /// <summary>
    /// This is the approval HTTP trigger function that will call the <see cref="DurableFunctionsOrchestration"/> orchestration accordingly with respective data from the <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="req">The request object of the GET API call.</param>
    /// <param name="orchestrationClient">The durable orchestration client.</param>
    /// <param name="log">The logger object.</param>
    /// <returns>Whether the request was approved or not.</returns>
    [FunctionName("HttpTriggerApproval")]
    public static async Task<HttpResponseMessage> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "approval")]
        HttpRequest req, [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        log.LogInformation($"Received an Approval Response");
        if (req.Query.TryGetValue("instanceId", out var instanceId) &&
            req.Query.TryGetValue("response", out var response))
        {
            log.LogInformation($"instanceId: '{instanceId}', response: '{response}'");
            bool isApproved = false;
            var status = await orchestrationClient.GetStatusAsync(instanceId.ToString());
            log.LogInformation($"Orchestration status: {status}");
            if (status != null && (status.RuntimeStatus == OrchestrationRuntimeStatus.Running ||
                                   status.RuntimeStatus == OrchestrationRuntimeStatus.Pending))
            {
                if (response.ToString().ToLower() == "approved")
                    isApproved = true;
                await orchestrationClient.RaiseEventAsync(instanceId, "ReceivedApprovalResponse", isApproved);

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Approved!") };
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Rejected!") };
        }

        return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Instance Id/Response is not provided.") };
    }
}