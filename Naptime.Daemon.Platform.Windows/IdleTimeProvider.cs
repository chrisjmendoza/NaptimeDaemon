using System;

namespace NaptimeDaemon.Platform.Windows;

public sealed class IdleTimeProvider
{
    public TimeSpan GetIdleTime()
    {
        var info = new NativeMethods.LASTINPUTINFO
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.LASTINPUTINFO>()
        };

        if (!NativeMethods.GetLastInputInfo(ref info))
            return TimeSpan.Zero;

        uint tickCount = NativeMethods.GetTickCount();
        uint idleTicks = tickCount - info.dwTime;

        return TimeSpan.FromMilliseconds(idleTicks);
    }
}
