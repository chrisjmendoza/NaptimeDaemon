using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using NaptimeDaemon.Core;
using NaptimeDaemon.Platform.Windows;

namespace NaptimeDaemon.App;

public partial class MainWindow : Window
{
    private readonly IdleTimeProvider _idleProvider = new();
    private readonly SleepController _sleepController = new();
    private readonly DispatcherTimer _timer;
    private readonly ConfigService _configService = new();

    private ConfigModel _config;
    private DateTime _configLastWriteUtc;

    public MainWindow()
    {
        InitializeComponent();

        _config = _configService.Load();
        _configLastWriteUtc = File.Exists(_configService.ConfigPath)
            ? File.GetLastWriteTimeUtc(_configService.ConfigPath)
            : DateTime.MinValue;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (File.Exists(_configService.ConfigPath))
        {
            var writeUtc = File.GetLastWriteTimeUtc(_configService.ConfigPath);
            if (writeUtc > _configLastWriteUtc)
            {
                _config = _configService.Load();
                _configLastWriteUtc = writeUtc;
            }
        }

        var idle = _idleProvider.GetIdleTime();

        IdleThresholdText.Text = $"Idle threshold: {TimeSpan.FromMinutes(_config.IdleThresholdMinutes):g}";

        var remaining = TimeSpan.FromMinutes(_config.IdleThresholdMinutes) - idle;
        if (remaining <= TimeSpan.Zero)
        {
            SleepTimerText.Text = "Now";
        }
        else
        {
            SleepTimerText.Text = remaining.ToString(@"hh\:mm\:ss");
        }
    }

    private void SleepNow_Click(object sender, RoutedEventArgs e)
    {
        _sleepController.Sleep();
    }

    private void EditConfig_Click(object sender, RoutedEventArgs e)
    {
        _ = _configService.Load();
        Process.Start(new ProcessStartInfo
        {
            FileName = _configService.ConfigPath,
            UseShellExecute = true
        });
    }
}
