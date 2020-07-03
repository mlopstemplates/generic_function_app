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
using System.Collections.Generic;

public static class GridEventHandler{

    private static string ParseEventGridValidationCode(dynamic requestObject)
    {
        var webhook_res = string.Empty;
        if (requestObject != null && requestObject[0]["data"] != null){
            var validationCode = requestObject[0].data.validationCode;
            if(validationCode != null){
                webhook_res = Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject {["validationResponse"]= validationCode});
            }
        }
        return webhook_res;
    }
    
    private static dynamic ParseMachineLearningEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    
    private static dynamic ParseBlobStorageEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    
    private static dynamic ParseContainerRegistryEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    
    [FunctionName("generic_triggers")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, ILogger log, ExecutionContext context)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await req.Content.ReadAsStringAsync();
        dynamic requestObject = JsonConvert.DeserializeObject(requestBody);
        var current_event = requestObject[0]["eventType"].ToString();

        if (current_event == "Microsoft.EventGrid.SubscriptionValidationEvent" ){
            string webhook_res = ParseEventGridValidationCode(requestObject);
            if(!string.IsNullOrEmpty(webhook_res)){
                return (ActionResult)new OkObjectResult($"{webhook_res}");
            }
        }

        string[] event_data = current_event.Split(".");
        string event_source = string.Empty;
        string event_type = string.Empty;
        
        if(event_data.Length>1){
            event_source = event_data[1];
        }

        var queryParams = System.Web.HttpUtility.ParseQueryString(req.RequestUri.Query);
        string repo_name = queryParams.Get("repoName");

        if(event_data.Length>2 && !string.IsNullOrEmpty(repo_name))
        {
            log.LogInformation("fetching repo name from query parameters."+repo_name);
            event_type = event_data[2].ToLower();
            var req_data = requestObject[0]["data"];
            
            if(event_source == "MachineLearningServices"){
                req_data = ParseMachineLearningEvent(requestObject);
            }
            if(event_source == "Storage"){
                req_data = ParseBlobStorageEvent(requestObject);
            }
            if(event_source == "ContainerRegistry"){
                req_data = ParseContainerRegistryEvent(requestObject);
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Awesome-Octocat-App");
                httpClient.DefaultRequestHeaders.Accept.Clear();

                var PATTOKEN =  Environment.GetEnvironmentVariable("PAT_TOKEN", EnvironmentVariableTarget.Process);
                
                httpClient.DefaultRequestHeaders.Add("Authorization", "token "+PATTOKEN);

                var client_payload = new Newtonsoft.Json.Linq.JObject { ["unit "] = false, ["integration"] = true, 
                                                ["data"] = req_data, ["event_source"] = event_source};
                
                var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject { ["event_type"] = event_type, ["client_payload"] = client_payload });
                
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("https://api.github.com/repos/"+repo_name+"/dispatches", content);
                var resultString = await response.Content.ReadAsStringAsync();
                return (ActionResult)new OkObjectResult("dispatch event sent");
            }
        }

        return (ActionResult)new OkObjectResult(current_event); 
    }
}
