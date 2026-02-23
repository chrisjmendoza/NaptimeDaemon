namespace NaptimeDaemon.Core;

public sealed class ConfigModel
{
    public int IdleThresholdMinutes { get; init; } = 120;

    public int RecheckIntervalSeconds { get; init; } = 15;

    public int CooldownAfterSleepMinutes { get; init; } = 10;

    public int CooldownAfterWakeMinutes { get; init; } = 2;

    public bool RequireNoFullscreen { get; init; } = false;

    public bool IgnoreIfMediaPlaying { get; init; } = false;

    public bool IgnoreIfRemoteSessionActive { get; init; } = false;

    public bool AllowSleepOnAC { get; init; } = true;

    public bool AllowSleepOnBattery { get; init; } = true;
}
