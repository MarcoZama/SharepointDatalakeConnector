using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharepointDatalakeConnector.ConfigModels;
using SharepointDatalakeConnector.Service;
using System.IO;
using System.Threading.Tasks;

namespace SharepointDatalakeFunction
{
    public class SharepointDatalakeFunction
    {
        private readonly ISharepointDatalakeService _sharepointDatalakeService;
        private readonly ILogger<SharepointDatalakeFunction> _log;
        private readonly DataLakeSettings _datalakeSettings;


        public SharepointDatalakeFunction(ISharepointDatalakeService sharepointDatalakeService,
            ILogger<SharepointDatalakeFunction> log,
            IOptions<DataLakeSettings> datalakeOptions)
        {
            _log = log;
            _datalakeSettings = datalakeOptions.Value;
            _sharepointDatalakeService= sharepointDatalakeService;
        }

        [FunctionName("SharepointDatalakeFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}

       

