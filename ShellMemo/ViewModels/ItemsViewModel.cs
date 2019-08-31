using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using ShellMemo.Models;
using ShellMemo.Views;
using ShellMemo.Services;
using System.ComponentModel;
using System.Linq;
using Xamarin.Essentials;

namespace ShellMemo.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private LuisService luisService = new LuisService();

        // 音声認識の開始・停止ボタンのテキスト
        private const string BUTTON_TEXT_START = "マイク";
        private const string BUTTON_TEXT_STOP = "終了";

        // 音声認識の開始・停止ボタンの表記
        private string _voiceRecognitionButtonText = BUTTON_TEXT_START;
        public string VoiceRecognitionButtonText
        {
            get { return _voiceRecognitionButtonText; }
            protected set { SetProperty(ref _voiceRecognitionButtonText, value); }
        }

        // 音声認識を実行中かどうか（trueなら実行中）
        private bool _isRecognizing;
        public bool IsRecognizing
        {
            get { return _isRecognizing; }
            protected set
            {
                // 音声認識が実行中の場合、音声認識ボタンのテキストを「停止」に変更する。
                // 音声認識が停止している場合は「開始」に変更する。
                VoiceRecognitionButtonText = value ? BUTTON_TEXT_STOP : BUTTON_TEXT_START;
                SetProperty(ref _isRecognizing, value);
            }
        }

        // 音声認識サービス
        private readonly IVoiceRecognitionService _voiceRecognitionService;

        public ObservableCollection<Item> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command VoiceRecognitionCommand { get; set; }

        public ItemsViewModel(IVoiceRecognitionService voiceRecognitionService)
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<NewItemPage, Item>(this, "AddItem", async (obj, item) =>
            {
                var newItem = item as Item;
                Items.Add(newItem);
                await DataStore.AddItemAsync(newItem);
            });

            _voiceRecognitionService = voiceRecognitionService;

            // 音声認識サービスのプロパティが変更されたときに実行する処理を設定する。
            _voiceRecognitionService.PropertyChanged += async (sender, e) => await voiceRecognitionServicePropertyChanged(sender, e);

            // 音声認識サービスの処理本体をコマンドに紐付ける。
            VoiceRecognitionCommand = new Command(ExecuteVoiceRecognition);
        }

        private async Task voiceRecognitionServicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "RecognizedText")
            {
                if (!string.IsNullOrEmpty(_voiceRecognitionService.RecognizedText))
                {
                    // LUISでインテント解析
                    var result = await luisService.DetectIntentAsync(_voiceRecognitionService.RecognizedText);

                    // メモ追加
                    if (result.TopScoringIntent.Intent == "AddIntent")
                    {
                        var item = new Item
                        {
                            Text = result.Entities.FirstOrDefault(e => e.Type == "text")?.Entity ?? "仮タイトル",
                            Description = result.Entities.FirstOrDefault(e => e.Type == "description")?.Entity ?? string.Empty
                        };
                        Items.Add(item);
                        await DataStore.AddItemAsync(item);

                    }
                    // 一覧読み上げ
                    else if (result.TopScoringIntent.Intent == "ListIntent")
                    {
                        var list = await DataStore.GetItemsAsync();
                        await TextToSpeech.SpeakAsync($"現在のメモの内容をおしらせします。{string.Join(",", list.Select(item => item.Text))}");
                    }
                    //LoadItemsCommand.Execute(null);
                }
            }
            else if (args.PropertyName == "IsRecognizing")
            {
                // 音声認識の実行状況変更がトリガーになった場合、その実行状況をViewModelに取得する。
                IsRecognizing = _voiceRecognitionService.IsRecognizing;
            }
        }

        // 音声認識サービス呼び出し用ボタンのコマンドの実処理
        private void ExecuteVoiceRecognition()
        {
            if (IsRecognizing)
            {
                // 音声認識を実行中の場合、「停止」ボタンとして機能させる。
                _voiceRecognitionService.StopRecognizing();
            }
            else
            {
                // 音声認識が停止中の場合、「開始」ボタンとして機能させる。
                _voiceRecognitionService.StartRecognizing();
            }
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}