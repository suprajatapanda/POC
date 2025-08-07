using PlatformCoreLib.Configuration;
using SIUtil;
namespace TRS.IT.TrsAppSettings;
public static class AppSettings
{
    public static void Initialize()
    {
        ConfigurationManager.Instance.Initialize();
    }

    public static string GetValue(string key)
    {
        var value = ConfigurationManager.Instance.GetValue(key);
        if (!string.IsNullOrEmpty(value))
        {
            Logger.LogMessage($"AppSettings.GetValue Key found: {key} Value: {value}");
        }
        else
        {
            Logger.LogMessage($"AppSettings.GetValue Key not found: {key}");
        }
        return value ?? string.Empty;
    }

    public static string GetConnectionString(string name)
    {
        var value = ConfigurationManager.Instance.GetConnectionString(name);
        if (string.IsNullOrEmpty(value))
        {
            Logger.LogMessage($"AppSettings.GetConnectionString not found: {name}");
        }
        return value ?? string.Empty;
    }

    public static string GetVaultValue(string key)
    {
        return ConfigurationManager.Instance.GetVaultValue(key) ?? string.Empty;
    }

    public static string GetPassword(string key)
    {
        var value = ConfigurationManager.Instance.GetPassword(key);
        if (!string.IsNullOrEmpty(value))
        {
            Logger.LogMessage($"AppSettings.GetValue Key found: {key} Value: {value}");
        }
        else
        {
            Logger.LogMessage($"AppSettings.GetValue Key not found: {key}");
        }
        return value ?? string.Empty;
    }
}
