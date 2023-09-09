using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsOrchestration
{
    /// <summary>
    /// This is the orchestration class.
    /// </summary>
    public static class DurableFunctionsOrchestration
    {
        /// <summary>
        /// This is the orchestration function that will be called from the start HTTP trigger.
        /// </summary>
        /// <param name="context">This contains the data of the orchestration instance like instance id, etc.</param>
        /// <param name="log">The logger object.</param>
        /// <returns></returns>
        [FunctionName("DurableFunctionsOrchestration")]
        public static async Task<bool> RunOrchestratorAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            log.LogInformation($"Initialised orchestration with ID = '{context.InstanceId}'.");
            var isApproved = false;

            using var timeoutCts = new CancellationTokenSource();
            int timeout = 45;
            DateTime expiration = context.CurrentUtcDateTime.AddSeconds(timeout);
            Task timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

            log.LogInformation($"Waiting for approval.");
            Task<bool> approvalResponse = context.WaitForExternalEvent<bool>("ReceivedApprovalResponse");
            Task winner = await Task.WhenAny(approvalResponse, timeoutTask);

            if (winner == approvalResponse)
            {
                log.LogInformation($"Received response ${approvalResponse.Result}.");
                if (approvalResponse.Result)
                {
                    isApproved = true;
                }
            }
            else
            {
                log.LogInformation($"Timed out.");
            }

            if (!timeoutTask.IsCompleted)
            {
                // All pending timers must be completed or cancelled before the function exits.
                timeoutCts.Cancel();
            }

            return isApproved;
        }
    }
}