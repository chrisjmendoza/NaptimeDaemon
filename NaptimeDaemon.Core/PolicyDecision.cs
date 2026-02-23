namespace NaptimeDaemon.Core;

public enum PolicyAction
{
    Wait,
    Sleep,
    Blocked,
    Snoozed
}

public sealed class PolicyDecision
{
    public PolicyAction Action { get; }
    public string Reason { get; }

    public PolicyDecision(PolicyAction action, string reason)
    {
        Action = action;
        Reason = reason;
    }

    public static PolicyDecision Wait(string reason) =>
        new(PolicyAction.Wait, reason);

    public static PolicyDecision Sleep(string reason) =>
        new(PolicyAction.Sleep, reason);

    public static PolicyDecision Blocked(string reason) =>
        new(PolicyAction.Blocked, reason);

    public static PolicyDecision Snoozed(string reason) =>
        new(PolicyAction.Snoozed, reason);
}
