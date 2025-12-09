using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform;
using Avalonia;
using Avalonia.Controls.Primitives;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using System.Text.Json;
using Avalonia.Input;

namespace LibraryApp.Views;

public partial class MenuResourcesView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly List<ResourceItem> _resources;

    public MenuResourcesView(List<ResourceItem> resources, MainWindow mainWindow)
    {
        InitializeComponent();
        _resources = resources;
        _mainWindow = mainWindow;
        ResourcesList.ItemsSource = _resources;
    }

    private async void OnResourceClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string url)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? url.Trim() :
                            OperatingSystem.IsMacOS() ? "open" :
                            "xdg-open",
                    Arguments = OperatingSystem.IsWindows() ? "" : url.Trim(),
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                
                Process.Start(psi);
                _mainWindow.HideSidebar(null, null);
            }
            catch { /* игнор */ }
        }
    }

    private void OnGoBack(object sender, RoutedEventArgs e) => _mainWindow.ShowMainMenu();
}