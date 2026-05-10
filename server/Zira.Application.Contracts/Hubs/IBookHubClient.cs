using System.Threading.Tasks;

namespace Zira.Hubs;

public interface IBookHubClient
{
    // Of course, user defined type is OK.
    Task ClientMethod1(string user, string message);
    Task ClientMethod2();
}
