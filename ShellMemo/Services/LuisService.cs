using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Newtonsoft.Json;

namespace ShellMemo.Services
{
    public class LuisService
    {
        HttpClient httpClient;
        public LuisService()
        {
            httpClient = new HttpClient();
        }

        public async Task<LuisResult> DetectIntentAsync(string query)
        {
            var json = await httpClient.GetStringAsync($"{Consts.LuisEndpointUrl}{Uri.EscapeDataString(query)}");
            return JsonConvert.DeserializeObject<LuisResult>(json);
        }
    }
}
