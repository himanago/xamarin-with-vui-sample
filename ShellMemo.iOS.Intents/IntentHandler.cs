using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Intents;
using Newtonsoft.Json;
using ShellMemo.Models;

namespace ShellMemo.iOS.Intents
{
    // As an example, this class is set up to handle Message intents.
    // You will want to replace this or add other intents as appropriate.
    // The intents you wish to handle must be declared in the extension's Info.plist.
    [Register("IntentHandler")]
    public class IntentHandler : INExtension, IINNotebookDomainHandling
    {
        protected IntentHandler(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override NSObject GetHandler(INIntent intent)
        {
            // This is the default implementation.  If you want different objects to handle different intents,
            // you can override this and return the handler you want for that particular intent.

            return this;
        }

        public void HandleAddTasks(INAddTasksIntent intent, Action<INAddTasksIntentResponse> completion)
        {
            throw new NotImplementedException();
        }

        public void HandleAppendToNote(INAppendToNoteIntent intent, Action<INAppendToNoteIntentResponse> completion)
        {
            throw new NotImplementedException();
        }

        public void HandleCreateNote(INCreateNoteIntent intent, Action<INCreateNoteIntentResponse> completion)
        {
            // データ登録
            var client = new HttpClient();
            client.BaseAddress = new Uri($"{Consts.AzureApiUrl}/");

            var item = new Item
            {
                Text = intent.Title.SpokenPhrase,
                Description = ((INTextNoteContent)intent.Content).Text
            };

            var serializedItem = JsonConvert.SerializeObject(item);

            Task.WaitAll(
                client.PostAsync($"api/item", new StringContent(serializedItem, Encoding.UTF8, "application/json"))
            );

            var userActivity = new NSUserActivity("INCreateNoteIntent");
            var response = new INCreateNoteIntentResponse(INCreateNoteIntentResponseCode.Success, userActivity);
            response.CreatedNote = new INNote(intent.Title, new[] { intent.Content }, null, null, null, null);
            completion(response);
        }

        public void HandleCreateTaskList(INCreateTaskListIntent intent, Action<INCreateTaskListIntentResponse> completion)
        {
            throw new NotImplementedException();
        }

        public void HandleSearchForNotebookItems(INSearchForNotebookItemsIntent intent, Action<INSearchForNotebookItemsIntentResponse> completion)
        {
            throw new NotImplementedException();
        }

        public void HandleSetTaskAttribute(INSetTaskAttributeIntent intent, Action<INSetTaskAttributeIntentResponse> completion)
        {
            throw new NotImplementedException();
        }
    }
}
