using Zira.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Zira.Permissions;

public class ZiraPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ZiraPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(ZiraPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(ZiraPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(ZiraPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(ZiraPermissions.Books.Delete, L("Permission:Books.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(ZiraPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ZiraResource>(name);
    }
}
