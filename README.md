# Async Approval System using Azure Durable Function
This is an asynchronous approval system created using Azure Durable Function with HTTP triggers and Azure Storage.

## Approval Workflow
1. To start the orchestration make a GET API call to `http://<baseUrl>/API/start`.
2. This will then go to the HTTP trigger which will start the orchestration.
3. As the orchestration is running it will wait for another approval GET API call to `http://<baseUrl>/API/approval?instanceId=<instance id>&response=<approval status: approved or rejected>` for 45 seconds.
4. Once the approval API call is made it will go to the HTTP trigger which will then go to the orchestration to approve or reject the process.
5. If there was no approval API call for 5 min after starting the orchestration, the process will be considered rejected.