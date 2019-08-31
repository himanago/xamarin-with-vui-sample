using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShellMemo.Common;

namespace ShellMemo.AmazonAlexa
{
    public class AmazonAlexaEndpoint
    {
        private AssistantService AssistantService { get; }

        public AmazonAlexaEndpoint(AssistantService assistantService)
        {
            AssistantService = assistantService;
        }

        [FunctionName(nameof(AmazonAlexaEndpoint))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            switch (skillRequest.Request)
            {
                case LaunchRequest lr:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = AssistantService.WelcomeMessage
                    };
                    break;
                case IntentRequest ir:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = await AssistantService.HandleIntentAsync(
                            ir.Intent.Name, ir.Intent.Slots.ToDictionary(s => s.Value.Name, s => s.Value.Value))
                    };
                    break;
                default:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = "すみません。わかりませんでした。"
                    };
                    break;
            }

            return new OkObjectResult(skillResponse);
        }
    }
}
