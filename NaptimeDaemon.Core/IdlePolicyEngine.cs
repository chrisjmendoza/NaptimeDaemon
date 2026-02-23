namespace NaptimeDaemon.Core;

public sealed class IdlePolicyEngine
{
    public PolicyDecision Evaluate(
        TimeSpan idleTime,
        DateTime utcNow,
        DateTime? lastSleepAttemptUtc,
        DateTime? snoozeUntilUtc,
        ConfigModel config)
    {
        if (snoozeUntilUtc.HasValue && utcNow < snoozeUntilUtc.Value)
        {
            return PolicyDecision.Snoozed("Snooze active");
        }

        if (idleTime < TimeSpan.FromMinutes(config.IdleThresholdMinutes))
        {
            return PolicyDecision.Wait("Idle threshold not reached");
        }

        if (lastSleepAttemptUtc.HasValue &&
            utcNow - lastSleepAttemptUtc.Value <
            TimeSpan.FromMinutes(config.CooldownAfterSleepMinutes))
        {
            return PolicyDecision.Wait("In cooldown after sleep attempt");
        }

        return PolicyDecision.Sleep("Idle threshold exceeded");
    }
}
