using System;
using System.Windows;
using System.Windows.Threading;
using NaptimeDaemon.Platform.Windows;

namespace NaptimeDaemon.App;

public partial class MainWindow : Window
{
    private readonly IdleTimeProvider _idleProvider = new();
    private readonly SleepController _sleepController = new();
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        var idle = _idleProvider.GetIdleTime();
        IdleTimeText.Text = idle.ToString(@"hh\:mm\:ss");
    }

    private void SleepNow_Click(object sender, RoutedEventArgs e)
    {
        _sleepController.Sleep();
    }
}
