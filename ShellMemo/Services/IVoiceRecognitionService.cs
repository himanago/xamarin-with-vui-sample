using System.ComponentModel;

namespace ShellMemo.Services
{
    public interface IVoiceRecognitionService : INotifyPropertyChanged
    {
        /// <summary>
        /// 音声認識が実行中かどうか（実行中の間のみtrueを返す）
        /// </summary>
        bool IsRecognizing { get; }

        /// <summary>
        /// 音声認識の結果テキスト（iOSの場合、認識結果をリアルタイムで取得できる）
        /// </summary>
        string RecognizedText { get; }

        /// <summary>
        /// 音声認識の開始
        /// </summary>
        void StartRecognizing();

        /// <summary>
        /// 音声認識の終了
        /// </summary>
        void StopRecognizing();
    }
}
