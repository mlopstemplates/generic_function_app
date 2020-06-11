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
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, ILogger log, ExecutionContext context)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await req.Content.ReadAsStringAsync();
        dynamic requestObject = JsonConvert.DeserializeObject(requestBody);
        var webhook_res = string.Empty;
        // log.LogInformation(requestObject);
        
        if (requestObject != null && requestObject[0]["data"] != null){
            log.LogInformation("I am here.");
            var validationCode = requestObject[0].data.validationCode;

            if(validationCode != null){
            webhook_res= Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject {["validationResponse"]= validationCode});
            return (ActionResult)new OkObjectResult($"{webhook_res}");
            }
        }

       
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Awesome-Octocat-App");
            httpClient.DefaultRequestHeaders.Accept.Clear();

            var PATTOKEN =  Environment.GetEnvironmentVariable("PATTOKEN", EnvironmentVariableTarget.Process);
            log.LogInformation($"PATTOKEN: {PATTOKEN}");

            httpClient.DefaultRequestHeaders.Add("Authorization", PATTOKEN);


            var client_payload = new Newtonsoft.Json.Linq.JObject { ["unit "] = false, ["integration"] = true, ["github_SHA"] = "7a6fe10d22b5c44be55698f6d123c6480451e18b"};

            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject { ["event_type"] = "deploy-command", ["client_payload"] = client_payload });
            
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("https://api.github.com/repos/cs1170353/test_mlops/dispatches", content);
            var resultString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(resultString);
            return (ActionResult)new OkObjectResult(resultString);
        }
    }
}
