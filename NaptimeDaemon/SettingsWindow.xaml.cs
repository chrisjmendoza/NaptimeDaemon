using System;
using System.Windows;
using NaptimeDaemon.Core;

namespace NaptimeDaemon.App;

public partial class SettingsWindow : Window
{
    private readonly ConfigService _configService;
    private ConfigModel _config;

    public event Action<ConfigModel>? ConfigSaved;

    public SettingsWindow(ConfigService configService)
    {
        InitializeComponent();

        _configService = configService;
        _config = _configService.Load();

        IdleMinutesBox.Text = _config.IdleThresholdMinutes.ToString();
    }

    private void Set5Min_Click(object sender, RoutedEventArgs e)
    {
        IdleMinutesBox.Text = "5";
    }

    private void Set60Min_Click(object sender, RoutedEventArgs e)
    {
        IdleMinutesBox.Text = "60";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(IdleMinutesBox.Text, out var minutes))
        {
            System.Windows.MessageBox.Show("Invalid number");
            return;
        }

        _config = new ConfigModel
        {
            IdleThresholdMinutes = minutes,
            RecheckIntervalSeconds = _config.RecheckIntervalSeconds,
            CooldownAfterSleepMinutes = _config.CooldownAfterSleepMinutes,
            CooldownAfterWakeMinutes = _config.CooldownAfterWakeMinutes,
            RequireNoFullscreen = _config.RequireNoFullscreen,
            IgnoreIfMediaPlaying = _config.IgnoreIfMediaPlaying,
            IgnoreIfRemoteSessionActive = _config.IgnoreIfRemoteSessionActive,
            AllowSleepOnAC = _config.AllowSleepOnAC,
            AllowSleepOnBattery = _config.AllowSleepOnBattery
        };

        _configService.Save(_config);

        ConfigSaved?.Invoke(_config);

        Close();
    }
}
