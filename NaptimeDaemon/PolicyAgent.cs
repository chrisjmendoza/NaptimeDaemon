using System;
using System.Windows.Threading;
using NaptimeDaemon.Core;
using NaptimeDaemon.Platform.Windows;

namespace NaptimeDaemon.App;

public sealed class PolicyAgent
{
    private readonly IdleTimeProvider _idleProvider = new();
    private readonly SleepController _sleepController = new();
    private readonly IdlePolicyEngine _policyEngine = new();
    private readonly DispatcherTimer _timer;

    private readonly ConfigModel _config;

    private DateTime? _lastSleepAttemptUtc;
    private DateTime? _snoozeUntilUtc;

    public PolicyAgent(ConfigModel config)
    {
        _config = config;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(config.RecheckIntervalSeconds)
        };

        _timer.Tick += OnTick;
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    public void Snooze(TimeSpan duration)
    {
        _snoozeUntilUtc = DateTime.UtcNow.Add(duration);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var idle = _idleProvider.GetIdleTime();
        var now = DateTime.UtcNow;

        var decision = _policyEngine.Evaluate(
            idle,
            now,
            _lastSleepAttemptUtc,
            _snoozeUntilUtc,
            _config);

        if (decision.Action == PolicyAction.Sleep)
        {
            _lastSleepAttemptUtc = now;
            _sleepController.Sleep();
        }
    }
}
