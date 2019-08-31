using LineDC.CEK;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ShellMemo.LineClova;
using ShellMemo.Common;

[assembly: FunctionsStartup(typeof(ShellMemo.Startup))]
namespace ShellMemo
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddClova<IMemoClova, MemoClova>()
                .AddHttpClient<AssistantService>();
        }
    }
}