using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using NaptimeDaemon.Core;
using Application = System.Windows.Application;

namespace NaptimeDaemon.App;

public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private PolicyAgent? _policyAgent;
    private System.Drawing.Icon? _appIcon;
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

        InitializeTrayIcon();

        var configService = new ConfigService();
        var config = configService.Load();
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
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(iconPath))
        {
            using var rawIcon = new System.Drawing.Icon(iconPath);
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
        contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closed += (_, _) => _mainWindow = null;
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        _notifyIcon!.Visible = false;
        _notifyIcon.Dispose();
        _appIcon?.Dispose();
        Shutdown();
    }
}
