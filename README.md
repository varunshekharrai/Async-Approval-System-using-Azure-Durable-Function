# Async Approval System using Azure Durable Function
This is an async approval system created using Azure Durable Function using HTTP triggers.

## Approval Workflow
1. To start the orchestration make a GET api call to `http://<baseUrl>/api/start`.
2. This will then go to the HTTP trigger which will start the orchestration.
3. As the orchestration is running it will wait for another approval GET api call to `http://<baseUrl>/api/approval?instanceId=<instance id>&response=<approval status: approved or rejected>`.
4. Once the approval api call is made it will go to the HTTP trigger which will approve or reject process.