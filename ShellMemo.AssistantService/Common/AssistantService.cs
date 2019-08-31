using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ShellMemo.Models;
using Newtonsoft.Json;
using System.Text;

namespace ShellMemo.Common
{
    public class AssistantService
    {
        public static readonly string WelcomeMessage = "メモアプリをはじめます。メモを登録するか、一覧を読み上げることができます。どうしますか？";

        public HttpClient Client { get; }

        public AssistantService(HttpClient client)
        {
            client.BaseAddress = new Uri(Consts.AzureApiUrl);
            Client = client;
        }

        public async Task<string> HandleIntentAsync(string intentName, Dictionary<string, string> slot = null)
        {
            switch (intentName)
            {
                case IntentNames.ListIntent:
                    var list = await GetItemsAsync();
                    return $"現在のメモの内容をおしらせします。{string.Join("。", list.Select(i => i.Text))}";

                case IntentNames.AddIntent:
                    await AddItemAsync(new Item
                    {
                        Text = slot["text"],
                        Description = slot["description"]
                    });
                    return $"{slot["text"]} というメモを追加しました。";

                default:
                    return "すみません、わかりませんでした。";
            }
        }

        private async Task<IEnumerable<Item>> GetItemsAsync()
        {
            var response = await Client.GetAsync("/api/item");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<IEnumerable<Item>>();
        }

        private async Task AddItemAsync(Item item)
        {
            var serializedItem = JsonConvert.SerializeObject(item);
            await Client.PostAsync($"api/item", new StringContent(serializedItem, Encoding.UTF8, "application/json"));
        }

    }
}
