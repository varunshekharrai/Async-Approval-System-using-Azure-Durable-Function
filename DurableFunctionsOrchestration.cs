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
    /// <summary>
    /// This is the orchestration class.
    /// </summary>
    public static class DurableFunctionsOrchestration
    {
        /// <summary>
        /// This is the orchestration function that will be called from the start HTTP trigger.
        /// </summary>
        /// <param name="context">This contains the data of the orchestration instance like instance id, etc.</param>
        /// <returns></returns>
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
    }
}