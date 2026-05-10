using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace Zira.Hubs;

[ExposeServices(typeof(BookHub))]
public class BookHub : AbpHub<IBookHubClient>, IBookHub
{
    private readonly IGuidGenerator _guidGenerator;

    public BookHub(IGuidGenerator guidGenerator)
    {
        _guidGenerator = guidGenerator;
    }

    public async Task<string> HubMethod1(string user, string message)
    {
        await Clients.All.ClientMethod1(
            $"Processed-{user}-{_guidGenerator.Create()}",
            $"Processed-{message}-{_guidGenerator.Create()}"
        );
        return "OK!";
    }

    public async Task HubMethod2()
    {
        await Clients.All.ClientMethod2();
    }
}
