using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using ShellMemo.Common;

namespace ShellMemo.LineClova
{
    public class LineClovaEndpoint
    {
        private IMemoClova Clova { get; }

        public LineClovaEndpoint(IMemoClova clova, AssistantService assistantService)
        {
            Clova = clova;
            Clova.Service = assistantService;
        }

        [FunctionName(nameof(LineClovaEndpoint))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            Clova.Logger = log;
            var response = await Clova.RespondAsync(req.Headers["SignatureCEK"], req.Body);
            return new OkObjectResult(response);
        }
    }
}
