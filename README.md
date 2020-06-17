# function_app

#### Events and its corresponding event types sent by the function:
```sh
  1.Microsoft.MachineLearningServices.ModelRegistered: model-registered
  2.Microsoft.MachineLearningServices.ModelDeployed: model-deployed
  3.Microsoft.MachineLearningServices.RunCompleted: model-updated
  4.Microsoft.MachineLearningServices.DatasetDriftDetected: data-drift-detected
  5.Microsoft.MachineLearningServices.RunStatusChanged: model-failed
```
  
### Example:
#### To trigger the workflow when the run is completed:
```sh
{
  On:
    repository_dispatch:
        types: [model-updated]
  (...)
}
```
