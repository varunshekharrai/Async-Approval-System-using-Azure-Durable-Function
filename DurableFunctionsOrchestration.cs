using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsOrchestration
{
    public static class DurableFunctionsOrchestration
    {
        [FunctionName("DurableFunctionsOrchestration")]
        public static async Task<bool> RunOrchestratorAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var isApproved = false;
            
            using (var timeoutCts = new CancellationTokenSource())
            {
                int timeout = 5;
                DateTime expiration = context.CurrentUtcDateTime.AddMinutes(timeout);
                Task timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

                Task<bool> approvalResponse = context.WaitForExternalEvent<bool>("ReceivedApprovalResponse");
                Task winner = await Task.WhenAny(approvalResponse, timeoutTask);

                if (winner == approvalResponse)
                {
                    if (approvalResponse.Result)
                    {
                        isApproved = true;
                    }
                    else
                    {
                        isApproved = false;
                    }
                }
                else
                {
                    isApproved = false;
                }

                if (!timeoutTask.IsCompleted)
                {
                    // All pending timers must be completed or cancelled before the function exits.
                    timeoutCts.Cancel();
                }

                return isApproved;
            }
        }

        [FunctionName("DurableFunctionsOrchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableFunctionsOrchestration", null);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}