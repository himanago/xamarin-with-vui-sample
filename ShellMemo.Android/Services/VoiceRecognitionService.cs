using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Speech;
using ShellMemo.Services;
using ShellMemo.ViewModels;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(ShellMemo.Droid.Services.VoiceRecognitionService))]
namespace ShellMemo.Droid.Services
{
    public class VoiceRecognitionService : IVoiceRecognitionService
    {
        protected bool SetProperty<T>(ref T backingStore, T value,
    [CallerMemberName]string propertyName = "",
    Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties

        // 音声認識の実行状況（実行中の間のみtrueを返す）
        private bool _isRecognizing;
        public bool IsRecognizing
        {
            get { return _isRecognizing; }
            set { SetProperty(ref _isRecognizing, value); }
        }

        // 音声認識の結果テキスト
        private string _recognizedText;
        public string RecognizedText
        {
            get
            {
                if (_recognizedText != null)
                    return _recognizedText;
                else
                    return string.Empty;
            }
            set { SetProperty(ref _recognizedText, value); }
        }

        #endregion

        #region Constant, MainActivity

        // 定数・MainActivity
        private readonly int REQUEST_CODE_VOICE = 10;       // 音声認識のリクエストコード
        private readonly int INTERVAL_1500_MILLISEC = 1500; // 1.5秒（ミリ秒単位）
        private MainActivity mainActivity;                  // MainActivity

        #endregion

        #region Constructor

        // コンストラクタ
        public VoiceRecognitionService()
        {
            // 音声認識のアクティビティで取得した結果をハンドルする処理をMainActivityに付ける。
            mainActivity = MainActivity.Instance;
            mainActivity.ActivityResult += HandleActivityResult;
        }

        #endregion
        
        #region Handler

        // 音声認識のアクティビティで取得した結果をハンドルする処理の本体
        private void HandleActivityResult(object sender, PreferenceManager.ActivityResultEventArgs args)
        {
            if (args.RequestCode == REQUEST_CODE_VOICE)
            {
                IsRecognizing = false;
                if (args.ResultCode == Result.Ok)
                {
                    // 認識が成功した場合、認識結果の文字列を引き出し、RecognizedTextに入れる。
                    var matches = args.Data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        RecognizedText = matches[0];
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        // 音声認識の開始処理
        public void StartRecognizing()
        {
            RecognizedText = string.Empty;
            IsRecognizing = true;

            try
            {
                // 音声認識のアクティビティを呼び出すためのインテントを用意する。
                var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);

                // 諸々のプロパティを設定する。
                voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "メモのタイトルと内容を言ってください。");
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, INTERVAL_1500_MILLISEC);
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, INTERVAL_1500_MILLISEC);
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, INTERVAL_1500_MILLISEC);
                voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                // 認識言語の指定。端末の設定言語(Java.Util.Locale.Default)で音声認識を行う。
                voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);

                // 音声認識のアクティビティを開始する。
                mainActivity.StartActivityForResult(voiceIntent, REQUEST_CODE_VOICE);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // 音声認識の停止処理
        public void StopRecognizing()
        {
            // Androidでは実装は不要。
        }

        #endregion
    }
}