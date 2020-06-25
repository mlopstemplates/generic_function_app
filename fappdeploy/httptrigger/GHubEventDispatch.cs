using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.EventGrid;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

public static class GridEventHandler{
    [FunctionName("PrettyPoisons")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, ILogger log, ExecutionContext context)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await req.Content.ReadAsStringAsync();
        dynamic requestObject = JsonConvert.DeserializeObject(requestBody);
        var webhook_res = string.Empty;
        var current_event = requestObject[0]["eventType"].ToString();

        if (current_event == "Microsoft.EventGrid.SubscriptionValidationEvent" ){
            if (requestObject != null && requestObject[0]["data"] != null){
                var validationCode = requestObject[0].data.validationCode;
                if(validationCode != null){
                webhook_res= Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject {["validationResponse"]= validationCode});
                return (ActionResult)new OkObjectResult($"{webhook_res}");
                }
            }
        }
        

        if (current_event.Contains("Microsoft.MachineLearningServices"))
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Awesome-Octocat-App");
                httpClient.DefaultRequestHeaders.Accept.Clear();

                var PATTOKEN =  Environment.GetEnvironmentVariable("PAT_TOKEN", EnvironmentVariableTarget.Process);
                // var repo_name = Environment.GetEnvironmentVariable("REPO_NAME", EnvironmentVariableTarget.Process);
                var repo_name = "";
                
                if(requestObject[0]["data"]["runTags"]==null || requestObject[0]["data"]["runTags"]["githuB_REPOSITORY"]==null)
                {
                    repo_name = Environment.GetEnvironmentVariable("REPO_NAME", EnvironmentVariableTarget.Process);
                    log.LogInformation("Fetching repo name from Environment variables.");
                }
                else
                {
                    repo_name = requestObject[0]["data"]["runTags"]["githuB_REPOSITORY"].ToString();
                    log.LogInformation("Fetching repo name from runTags");
                }

                httpClient.DefaultRequestHeaders.Add("Authorization", "token "+PATTOKEN);

                var client_payload = new Newtonsoft.Json.Linq.JObject { ["unit "] = false, ["integration"] = true, ["data"] = requestObject[0]["data"]};
                var event_types = "unknown";

                if(current_event == "Microsoft.MachineLearningServices.RunCompleted")
                {
                    event_types = "run-completed";
                }
                else if(current_event == "Microsoft.MachineLearningServices.RunStatusChanged")
                {
                    event_types = "run-status-changed";
                }
                else if(current_event == "Microsoft.MachineLearningServices.ModelRegistered")
                {
                    event_types = "model-registered";
                }
                else if(current_event == "Microsoft.MachineLearningServices.ModelDeployed")
                {
                    event_types = "model-deployed";
                }
                else if(current_event == "Microsoft.MachineLearningServices.DatasetDriftDetected")
                {
                    event_types = "data-drift-detected";
                }
                else
                {
                    event_types = "unknown";
                }

                var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject { ["event_type"] = event_types, ["client_payload"] = client_payload });
                
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("https://api.github.com/repos/"+repo_name+"/dispatches", content);
                var resultString = await response.Content.ReadAsStringAsync();
                return (ActionResult)new OkObjectResult("dispatch event sent");
            }
        }

       return (ActionResult)new OkObjectResult(current_event); 
    }
}
