using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Resources;
using NaptimeDaemon.Core;
using Application = System.Windows.Application;

namespace NaptimeDaemon.App;

public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private PolicyAgent? _policyAgent;
    private ConfigService? _configService;
    private WakeDiagnosticsService? _wakeDiagnostics;
    private System.Drawing.Icon? _appIcon;
    private MemoryStream? _appIconStream;
    private Mutex? _singleInstanceMutex;

    private const string SingleInstanceMutexName = "Local\\NaptimeDaemon.App";

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, name: SingleInstanceMutexName, createdNew: out var createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);

        _wakeDiagnostics = new WakeDiagnosticsService();
        _wakeDiagnostics.CaptureWakeInfo();

        InitializeTrayIcon();

        _configService = new ConfigService();
        var config = _configService.Load();
        _policyAgent = new PolicyAgent(config);
        _policyAgent.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;

        base.OnExit(e);
    }

    private void InitializeTrayIcon()
    {
        var iconUri = new Uri("pack://application:,,,/Assets/AppIcon.ico", UriKind.Absolute);
        StreamResourceInfo? iconResource = GetResourceStream(iconUri);
        if (iconResource is not null)
        {
            _appIconStream = new MemoryStream();
            using (iconResource.Stream)
            {
                iconResource.Stream.CopyTo(_appIconStream);
            }
            _appIconStream.Position = 0;

            using var rawIcon = new System.Drawing.Icon(_appIconStream);
            _appIcon = new System.Drawing.Icon(rawIcon, new System.Drawing.Size(32, 32));
        }
        else
        {
            _appIcon = null;
        }

        _notifyIcon = new NotifyIcon
        {
            Icon = _appIcon ?? System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "NaptimeDaemon"
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Open", null, (_, _) => ShowMainWindow());
        contextMenu.Items.Add("Settings", null, (_, _) => ShowSettings());
        contextMenu.Items.Add("View Wake Log", null, (_, _) =>
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NaptimeDaemon",
                "wake-log.txt");

            if (File.Exists(path))
                Process.Start("notepad.exe", path);
        });
        contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow(ApplyConfig);
            _mainWindow.Closed += (_, _) => _mainWindow = null;
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void ShowSettings()
    {
        if (_configService == null) return;

        var window = new SettingsWindow(_configService);

        window.ConfigSaved += ApplyConfig;

        window.Show();
    }

    private void ApplyConfig(ConfigModel config)
    {
        _policyAgent?.Stop();
        _policyAgent = new PolicyAgent(config);
        _policyAgent.Start();
    }

    private void ExitApplication()
    {
        _notifyIcon!.Visible = false;
        _notifyIcon.Dispose();
        _appIcon?.Dispose();
        _appIconStream?.Dispose();
        Shutdown();
    }
}
