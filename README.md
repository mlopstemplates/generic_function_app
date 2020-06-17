# function_app
This repository contains code for Azure Function app which includes an Http Trigger function. The function can send github repository dispatch event when triggered. It is especially modelled to send Azure machine learning events when subscribed to the event grid of the workspace with the endpoint as the function url. 

#### Basic Requirements to use the function:
1. Add personal access token in the application settings of the function app with the name **PAT_TOKEN**.
2. Add owner/repo name in the application settings of the function app with the name **REPO_NAME**.

### Events and its corresponding event types sent by the function:
```sh
  1.Microsoft.MachineLearningServices.ModelRegistered: model-registered
  2.Microsoft.MachineLearningServices.ModelDeployed: model-deployed
  3.Microsoft.MachineLearningServices.RunCompleted: run-completed
  4.Microsoft.MachineLearningServices.DatasetDriftDetected: data-drift-detected
  5.Microsoft.MachineLearningServices.RunStatusChanged: run-status-changed
```
  
### Example:
#### To trigger the workflow when the run is completed:
```sh

  On:
    repository_dispatch:
        types: [run-completed]
  (...)

```
