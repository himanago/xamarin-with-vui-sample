using LineDC.CEK;
using Microsoft.Extensions.Logging;
using ShellMemo.Common;

namespace ShellMemo.LineClova
{
    public interface IMemoClova : IClova
    {
        ILogger Logger { get; set; }
        AssistantService Service { get; set; }
    }
}
