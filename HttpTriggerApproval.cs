using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsOrchestration;

public static class HttpTriggerApproval
{
    [FunctionName("HttpTriggerApproval")]
    public static async Task RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "approval")] HttpRequest req, [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        log.LogInformation($"Received an Approval Response");
        if(req.Query.TryGetValue("instanceId", out var instanceId) && req.Query.TryGetValue("response", out var response))
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
            }
        }
    }
}