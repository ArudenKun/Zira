using System.Threading.Tasks;

namespace Zira.Hubs;

public interface IBookHub
{
    Task<string> HubMethod1(string user, string message);
    Task HubMethod2();
}
