using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

    private void OnResourceClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start("explorer", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else // Linux
                {
                    Process.Start("xdg-open", url);
                }
            }
            catch 
            {
                // Игнор ошибок для MVP
            }
        }
    }

    private void OnGoBack(object sender, RoutedEventArgs e) => _mainWindow.ShowMainMenu();
}