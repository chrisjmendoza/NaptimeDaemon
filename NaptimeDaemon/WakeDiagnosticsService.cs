using System;
using System.Diagnostics;
using System.IO;

namespace NaptimeDaemon.App;

public sealed class WakeDiagnosticsService
{
    private readonly string _logPath;

    public WakeDiagnosticsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "NaptimeDaemon");

        Directory.CreateDirectory(folder);

        _logPath = Path.Combine(folder, "wake-log.txt");
    }

    public void CaptureWakeInfo()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var lastWake = RunCommand("powercfg", "/lastwake");
            var wakeTimers = RunCommand("powercfg", "/waketimers");
            var devices = RunCommand("powercfg", "/devicequery wake_armed");

            var log = $"""
==============================
[{timestamp}]
--- LAST WAKE ---
{lastWake}

--- WAKE TIMERS ---
{wakeTimers}

--- WAKE-ARMED DEVICES ---
{devices}

""";

            File.AppendAllText(_logPath, log);
        }
        catch
        {
            // Fail silently — logging should never crash the app
        }
    }

    private static string RunCommand(string file, string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        return process?.StandardOutput.ReadToEnd() ?? "";
    }
}
