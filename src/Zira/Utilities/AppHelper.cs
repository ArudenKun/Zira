using Zira.Extensions;

namespace Zira.Utilities;

public static class AppHelper
{
    public const string Name = "Zira";
    public static bool IsDebug
#if DEBUG
        => true;
#else
        => false;
#endif
    public static string AppDir => AppDomain.CurrentDomain.BaseDirectory;

    public static string RoamingDir =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static string DataDir
    {
        get
        {
            if (
                File.Exists(AppDir.CombinePath(".portable"))
                || Directory.Exists(AppDir.CombinePath("data"))
                || IsDebug
            )
            {
                var dataDir = AppDir.CombinePath("data");
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }
                return dataDir;
            }
            return RoamingDir.CombinePath(Name);
        }
    }

    public static string LogsDir => DataDir.CombinePath("logs");

    public static string SettingsPath => DataDir.CombinePath("appsettings.json");
}
