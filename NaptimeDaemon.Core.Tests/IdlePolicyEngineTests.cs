using NaptimeDaemon.Core;
using Xunit;

namespace NaptimeDaemon.Core.Tests;

public sealed class IdlePolicyEngineTests
{
    private static readonly ConfigModel DefaultConfig = new()
    {
        IdleThresholdMinutes = 120,
        CooldownAfterSleepMinutes = 10
    };

    [Fact]
    public void Evaluate_WhenIdleBelowThreshold_ReturnsWait()
    {
        var engine = new IdlePolicyEngine();
        var utcNow = new DateTime(2030, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        var decision = engine.Evaluate(
            idleTime: TimeSpan.FromMinutes(119),
            utcNow: utcNow,
            lastSleepAttemptUtc: null,
            snoozeUntilUtc: null,
            config: DefaultConfig);

        Assert.Equal(PolicyAction.Wait, decision.Action);
    }

    [Fact]
    public void Evaluate_WhenIdleExceedsThresholdAndNoCooldown_ReturnsSleep()
    {
        var engine = new IdlePolicyEngine();
        var utcNow = new DateTime(2030, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        var decision = engine.Evaluate(
            idleTime: TimeSpan.FromMinutes(121),
            utcNow: utcNow,
            lastSleepAttemptUtc: null,
            snoozeUntilUtc: null,
            config: DefaultConfig);

        Assert.Equal(PolicyAction.Sleep, decision.Action);
    }

    [Fact]
    public void Evaluate_WhenLastSleepAttemptWithinCooldown_ReturnsWait()
    {
        var engine = new IdlePolicyEngine();
        var utcNow = new DateTime(2030, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var lastSleepAttemptUtc = utcNow.AddMinutes(-9);

        var decision = engine.Evaluate(
            idleTime: TimeSpan.FromMinutes(500),
            utcNow: utcNow,
            lastSleepAttemptUtc: lastSleepAttemptUtc,
            snoozeUntilUtc: null,
            config: DefaultConfig);

        Assert.Equal(PolicyAction.Wait, decision.Action);
    }

    [Fact]
    public void Evaluate_WhenSnoozeUntilInFuture_ReturnsSnoozed()
    {
        var engine = new IdlePolicyEngine();
        var utcNow = new DateTime(2030, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var snoozeUntilUtc = utcNow.AddMinutes(5);

        var decision = engine.Evaluate(
            idleTime: TimeSpan.FromMinutes(500),
            utcNow: utcNow,
            lastSleepAttemptUtc: null,
            snoozeUntilUtc: snoozeUntilUtc,
            config: DefaultConfig);

        Assert.Equal(PolicyAction.Snoozed, decision.Action);
    }

    [Fact]
    public void Evaluate_WhenSnoozeUntilInPastAndIdleExceedsThreshold_ReturnsSleep()
    {
        var engine = new IdlePolicyEngine();
        var utcNow = new DateTime(2030, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var snoozeUntilUtc = utcNow.AddSeconds(-1);

        var decision = engine.Evaluate(
            idleTime: TimeSpan.FromMinutes(121),
            utcNow: utcNow,
            lastSleepAttemptUtc: null,
            snoozeUntilUtc: snoozeUntilUtc,
            config: DefaultConfig);

        Assert.Equal(PolicyAction.Sleep, decision.Action);
    }
}
