namespace NaptimeDaemon.Platform.Windows;

public sealed class SleepController
{
    public bool Sleep()
    {
        // hibernate: false
        // forceCritical: true
        // disableWakeEvent: false
        return NativeMethods.SetSuspendState(false, true, false);
    }
}
