using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using System;
using Avalonia;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LibraryApp.Views;
namespace LibraryApp.Views;

public partial class DevModeWindow : Window
{
    private readonly MainWindow _mainWindow;

    public DevModeWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        LoadInfo();
    }

    private void LoadInfo()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        var mode = _mainWindow._config.AppSettings.CurrentMode;
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        
        InfoText.Text = $@"
Версия: {version}
Текущий режим: {mode}
Путь к конфигу: {configPath}
ОС: {Environment.OSVersion}
Время запуска: {DateTime.Now:HH:mm:ss dd.MM.yyyy}
GitHub: https://github.com/Z9TONEDEVELOPER/LibraryApplication";
    }

    private void ToggleMode(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var current = _mainWindow._config.AppSettings.CurrentMode;
        _mainWindow._config.AppSettings.CurrentMode = current == "Adults" ? "Kids" : "Adults";
        _mainWindow.ApplyBackground();
        LoadInfo();
    }

    private void RefreshBackground(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.ApplyBackground();
    }

    private void RestartApp(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Перезапуск приложения
        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath,
            UseShellExecute = true
        };
        Process.Start(startInfo);
        _mainWindow.Close();
    }

    private void CloseWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }


    private void CloseApplication(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
            return;
        }

        
        Close();
    }

}