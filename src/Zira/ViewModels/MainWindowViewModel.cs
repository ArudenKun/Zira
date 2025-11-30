using System.ComponentModel;
using System.Net.Http;
using Duende.IdentityModel.Client;
using Zira.Books;

namespace Zira.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MainWindowViewModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        var client = _httpClientFactory.CreateClient();
        var document = await client.GetDiscoveryDocumentAsync();
    }

    public string Greeting { get; } = "Welcome to Avalonia!";
}
