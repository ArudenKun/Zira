using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Zira.Models.EventData;

namespace Zira.ViewModels;

public sealed partial class LoginViewModel : ViewModel, ISingletonDependency
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoginEnabled))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyDataErrorInfo]
    [Required(AllowEmptyStrings = false)]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoginEnabled))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyDataErrorInfo]
    [Required(AllowEmptyStrings = false)]
    [PasswordPropertyText]
    public partial string Password { get; set; } = string.Empty;

    public bool IsLoginEnabled =>
        !UserName.IsNullOrEmpty()
        && !UserName.IsNullOrWhiteSpace()
        && !Password.IsNullOrEmpty()
        && !Password.IsNullOrWhiteSpace();

    [RelayCommand(CanExecute = nameof(IsLoginEnabled))]
    private async Task LoginAsync() =>
        await SetBusyAsync(async () =>
        {
            try
            {
                var result = await AuthenticationManager.AuthenticateAsync(UserName, Password);
                if (result.IsSuccess)
                {
                    await LocalEventBus.PublishAsync(new LoginSuccessEventData());
                }
                else
                {
                    ToastService.ShowToast(
                        NotificationType.Error,
                        "Login Failed",
                        result.Error ?? "Please check your username or password"
                    );
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowExceptionToast(ex, "Login Failed");
                Logger.LogException(ex, LogLevel.Warning);
            }
        });
}
