using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lucide.Avalonia;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Zira.Books;
using Zira.Permissions;
using Zira.Services;

namespace Zira.ViewModels.Pages;

public sealed partial class HomePageViewModel : PageViewModel, ISingletonDependency
{
    private readonly IBookAppService _bookAppService;
    private readonly IAuthenticationManager _authenticationManager;
    private readonly IUserPermissionManager _userPermissionManager;
    private readonly IIdentityUserAppService _identityUserAppService;

    public HomePageViewModel(
        IBookAppService bookAppService,
        IAuthenticationManager authenticationManager,
        IUserPermissionManager userPermissionManager,
        IIdentityUserAppService identityUserAppService
    )
    {
        _bookAppService = bookAppService;
        _authenticationManager = authenticationManager;
        _userPermissionManager = userPermissionManager;
        _identityUserAppService = identityUserAppService;
    }

    public override int Index => 1;
    public override string DisplayName => "Home";
    public override LucideIconKind IconKind => LucideIconKind.House;

    [ObservableProperty]
    public partial string AccessToken { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        await _authenticationManager.AuthenticateAsync(UserName, Password);
        var identityUserDto = await _identityUserAppService.FindByUsernameAsync(UserName);
        await _userPermissionManager.GrantUserPermissionAsync(
            identityUserDto.Id,
            ZiraPermissions.Books.Default
        );
        AccessToken = identityUserDto.UserName + "-" + $"{identityUserDto.Id}";
    }

    [RelayCommand]
    private async Task CreateBookAsync()
    {
        var output = await _bookAppService.CreateAsync(
            new CreateUpdateBookDto
            {
                Name = Guid.CreateVersion7().ToString(),
                Price = 10f,
                PublishDate = DateTime.Now,
                Type = BookType.Biography,
            }
        );

        AccessToken = output.Id.ToString();
    }
}
