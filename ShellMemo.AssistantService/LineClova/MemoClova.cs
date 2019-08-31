using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LineDC.CEK;
using LineDC.CEK.Models;
using Microsoft.Extensions.Logging;
using ShellMemo.Common;

namespace ShellMemo.LineClova
{
    public class MemoClova : ClovaBase, IMemoClova
    {
        public ILogger Logger { get; set; }
        public AssistantService Service { get; set; }

        protected override async Task OnLaunchRequestAsync(Session session, CancellationToken cancellationToken)
        {
            // Clovaは生テキストをそのままとれないのでメモ追加は不可能なので、リスト読み上げだけ。
            // TODO LINEへの送信、削除、件数くらいはできそうだけど順序などもってないので今回はなし
            var message = await Service.HandleIntentAsync(IntentNames.ListIntent);
            Response.AddText(message);
        }
    }
}
