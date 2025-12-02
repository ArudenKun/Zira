using AutoInterfaceAttributes;
using JetBrains.Annotations;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;

namespace Zira.Services;

[AutoInterface]
[UsedImplicitly]
public sealed class UserPermissionManager : IUserPermissionManager, ITransientDependency
{
    private readonly IPermissionAppService _permissionAppService;

    public UserPermissionManager(IPermissionAppService permissionAppService)
    {
        _permissionAppService = permissionAppService;
    }

    public async Task GrantUserPermissionAsync(Guid userId, string permissionName)
    {
        var updateDto = new UpdatePermissionsDto
        {
            Permissions = [new UpdatePermissionDto { Name = permissionName, IsGranted = true }],
        };

        await _permissionAppService.UpdateAsync(
            UserPermissionValueProvider.ProviderName,
            userId.ToString(),
            updateDto
        );
    }
}
