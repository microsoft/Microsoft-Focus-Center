using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AnonymizationFunctionApp.Commands;
using System.Net;
using System.Linq;
using AnonymizationFunctionApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace AnonymizationFunctionApp
{
    public class AnonymizationFunction
    {
        private readonly IAnonymizationCommand _anonymizationCommand;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AnonymizationFunction> _logger;

        /// <summary>
        /// Parameterized Constructor for Initializing AnonymizationCommand and Configuration Object
        /// </summary>
        /// <param name="anonymizationCommand"></param>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public AnonymizationFunction(IAnonymizationCommand anonymizationCommand, ILogger<AnonymizationFunction> logger, IConfiguration configuration)
        {
            _anonymizationCommand = anonymizationCommand;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Entry point function for Anonymization Process.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("AnonymizationFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = null)] HttpRequest req,
            ILogger log)
        {
           
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            try
            {
                //read from Azure http body
                InputRequestModel requestModel = JsonConvert.DeserializeObject<InputRequestModel>(requestBody);
                var result = await _anonymizationCommand.Execute(requestModel);

                switch (result.Status)
                {
                    case CommandStatusCode.Succeeded:
                        return new OkObjectResult(HttpStatusCode.OK);
                    default:
                        return new BadRequestObjectResult(HttpStatusCode.InternalServerError);
                }

            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error on requestModel binding : " + ex.Message);
                throw;
            }
        }
    }
}
