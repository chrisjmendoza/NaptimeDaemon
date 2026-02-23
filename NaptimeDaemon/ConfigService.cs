using System;
using System.IO;
using System.Text.Json;
using NaptimeDaemon.Core;

namespace NaptimeDaemon.App;

public sealed class ConfigService
{
    private readonly string _configPath;

    public string ConfigPath => _configPath;

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "NaptimeDaemon");

        Directory.CreateDirectory(folder);

        _configPath = Path.Combine(folder, "config.json");
    }

    public ConfigModel Load()
    {
        if (!File.Exists(_configPath))
        {
            var defaultConfig = new ConfigModel();
            Save(defaultConfig);
            return defaultConfig;
        }

        var json = File.ReadAllText(_configPath);
        return JsonSerializer.Deserialize<ConfigModel>(json) ?? new ConfigModel();
    }

    public void Save(ConfigModel config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_configPath, json);
    }
}
