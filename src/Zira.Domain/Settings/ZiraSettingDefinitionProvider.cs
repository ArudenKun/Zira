using Volo.Abp.Settings;

namespace Zira.Settings;

public class ZiraSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(ZiraSettings.MySetting1));
    }
}
