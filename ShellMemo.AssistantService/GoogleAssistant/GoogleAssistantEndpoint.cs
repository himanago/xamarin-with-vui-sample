using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ShellMemo.Common;

namespace ShellMemo.GoogleAssistant
{
    public class GoogleAssistantEndpoint
    {
        private AssistantService AssistantService { get; }

        public GoogleAssistantEndpoint(AssistantService assistantService)
        {
            AssistantService = assistantService;
        }

        [FunctionName(nameof(GoogleAssistantEndpoint))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            var webhookRequest = parser.Parse<WebhookRequest>(await req.ReadAsStringAsync());

            var webhookResponse = new WebhookResponse
            {
                FulfillmentText = (webhookRequest.QueryResult.Intent.DisplayName == "Default Welcome Intent")
                    ? AssistantService.WelcomeMessage
                    : await AssistantService.HandleIntentAsync(
                        webhookRequest.QueryResult.Intent.DisplayName,
                        webhookRequest.QueryResult.Parameters.Fields.ToDictionary(f => f.Key, f =>f.Value.StringValue))
            };

            return new ProtcolBufJsonResult(webhookResponse, JsonFormatter.Default);
        }
    }
}