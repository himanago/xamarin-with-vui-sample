using System;
using System.Threading.Tasks;
using ShellMemo.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ShellMemo.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Item Item { get; set; }
        public Command SpeechCommand { get; set; }

        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;
            SpeechCommand = new Command(async () => await SpeakAsync());
        }

        private async Task SpeakAsync()
        {
            await TextToSpeech.SpeakAsync($"{Item.Text}, {Item.Description}");
        }
    }
}
