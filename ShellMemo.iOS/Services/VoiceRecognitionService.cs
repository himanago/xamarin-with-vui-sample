using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AVFoundation;
using Foundation;
using ShellMemo.Services;
using ShellMemo.ViewModels;
using Speech;

[assembly: Xamarin.Forms.Dependency(typeof(ShellMemo.iOS.Services.VoiceRecognitionService))]
namespace ShellMemo.iOS.Services
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

        // 以下は https://qiita.com/microwavePC/items/40b1016cf84ea89c4ed1 の実装をそのまま

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

        #region Variables

        // 音声認識に必要な諸々のクラスのインスタンス。
        private AVAudioEngine audioEngine;
        private SFSpeechRecognizer speechRecognizer;
        private SFSpeechAudioBufferRecognitionRequest recognitionRequest;
        private SFSpeechRecognitionTask recognitionTask;

        #endregion

        #region Public Methods

        // 音声認識の開始処理
        public void StartRecognizing()
        {
            RecognizedText = string.Empty;
            IsRecognizing = true;

            // 音声認識の許可をユーザーに求める。
            SFSpeechRecognizer.RequestAuthorization((SFSpeechRecognizerAuthorizationStatus status) =>
            {
                switch (status)
                {
                    case SFSpeechRecognizerAuthorizationStatus.Authorized:
                        // 音声認識がユーザーに許可された場合、必要なインスタンスを生成した後に音声認識の本処理を実行する。
                        // SFSpeechRecognizerのインスタンス生成時、コンストラクタの引数でlocaleを指定しなくても、
                        // 端末の標準言語が日本語なら日本語は問題なく認識される。
                        audioEngine = new AVAudioEngine();
                        speechRecognizer = new SFSpeechRecognizer();
                        recognitionRequest = new SFSpeechAudioBufferRecognitionRequest();
                        startRecognitionSession();
                        break;
                    default:
                        // 音声認識がユーザーに許可されなかった場合、処理を終了する。
                        return;
                }
            }
            );
        }

        // 音声認識の停止処理
        public void StopRecognizing()
        {
            try
            {
                audioEngine?.Stop();
                recognitionTask?.Cancel();
                recognitionRequest?.EndAudio();
                IsRecognizing = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        // 音声認識の本処理
        private void startRecognitionSession()
        {
            // 音声認識のパラメータ設定と認識開始。ここのパラメータはおまじない。
            audioEngine.InputNode.InstallTapOnBus(
                bus: 0,
                bufferSize: 1024,
                format: audioEngine.InputNode.GetBusOutputFormat(0),
                tapBlock: (buffer, when) => { recognitionRequest?.Append(buffer); }
            );
            audioEngine?.Prepare();
            NSError error = null;
            audioEngine?.StartAndReturnError(out error);
            if (error != null)
            {
                Console.WriteLine(error);
                return;
            }

            try
            {
                if (recognitionTask?.State == SFSpeechRecognitionTaskState.Running)
                {
                    // 音声認識が実行中に音声認識開始処理が呼び出された場合、実行中だった音声認識を中断する。
                    recognitionTask.Cancel();
                }

                recognitionTask = speechRecognizer.GetRecognitionTask(recognitionRequest,
                    (SFSpeechRecognitionResult result, NSError err) =>
                    {
                        if (result == null)
                        {
                            // iOS Simulator等、端末が音声認識に対応していない場合はここに入る。
                            StopRecognizing();
                            return;
                        }

                        if (err != null)
                        {
                            Console.WriteLine(err);
                            StopRecognizing();
                            return;
                        }

                        if ((result.BestTranscription != null) && (result.BestTranscription.FormattedString != null))
                        {
                            // 音声を認識できた場合、認識結果を更新する。
                            RecognizedText = result.BestTranscription.FormattedString;
                        }

                        if (result.Final)
                        {
                            // 音声が認識されなくなって時間が経ったら音声認識を打ち切る。
                            StopRecognizing();
                            return;
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
