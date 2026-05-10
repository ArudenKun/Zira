using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;
using Volo.Abp.DependencyInjection;
using Zira.Hubs;

namespace Zira.ViewModels;

public sealed partial class MainWindowViewModel
    : ViewModel,
        IBookHubClient,
        IHubConnectionObserver,
        IAsyncDisposable,
        ISingletonDependency
{
    private readonly HubConnection _connection;
    private readonly IBookHub _bookHub;
    private readonly IDisposable _subscription;

    public MainWindowViewModel()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(
                "https://localhost:44395/signalr-hubs/book",
                options =>
                    options.AccessTokenProvider = () => Task.FromResult<string?>(string.Empty)
            )
            .WithAutomaticReconnect()
            .Build();

        _bookHub = _connection.CreateHubProxy<IBookHub>();
        _subscription = _connection.Register<IBookHubClient>(this);
    }

    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";

    private bool CanStartConnection() => _connection.State is not HubConnectionState.Connected;

    [RelayCommand(CanExecute = nameof(CanStartConnection))]
    private async Task StartConnectionAsync()
    {
        await _connection.StartAsync();
        StopConnectionCommand.NotifyCanExecuteChanged();
    }

    private bool CanStopConnection() => _connection.State is HubConnectionState.Connected;

    [RelayCommand(CanExecute = nameof(CanStopConnection))]
    private async Task StopConnectionAsync()
    {
        await _connection.StopAsync();
        StartConnectionCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task ServerMethodAsync()
    {
        await _bookHub.HubMethod1($"ServerMethodUser", $"ServerMethodMessage");
    }

    public Task ClientMethod1(string user, string message)
    {
        Greeting = $"Received: {user}-{message}";
        return Task.CompletedTask;
    }

    public Task ClientMethod2()
    {
        Greeting = Guid.CreateVersion7().ToString();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _subscription.Dispose();

        await _connection.StopAsync();
        await _connection.DisposeAsync();
    }

    public Task OnClosed(Exception? exception)
    {
        return Task.CompletedTask;
    }

    public Task OnReconnected(string? connectionId)
    {
        return Task.CompletedTask;
    }

    public Task OnReconnecting(Exception? exception)
    {
        return Task.CompletedTask;
    }
}
