using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;

namespace Clandom.Service.Settings;

public static class SettingsManager
{
    private const string ConfigFileName = "settings.json";

    public static AppSettings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(ConfigFileName, optional: true, reloadOnChange: false)
            .Build();

        var settings = new AppSettings();
        configuration.Bind(settings);
        return settings;
    }

    public static void Save(AppSettings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(ConfigFileName, json);
    }
}