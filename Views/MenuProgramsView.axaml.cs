using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using Avalonia;
namespace LibraryApp.Views;

public partial class MenuProgramsView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly List<ProgramItem> _programs;

    public MenuProgramsView(List<ProgramItem> programs, MainWindow mainWindow)
    {
        InitializeComponent();
        _programs = programs;
        _mainWindow = mainWindow;
        ProgramsList.ItemsSource = _programs;
    }

    private void OnProgramClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string rawPath)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{rawPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else
                {
                    Process.Start(rawPath);
                }
                _mainWindow.HideSidebar(null, null); // Используем новый public метод
            }
            catch { /* игнор */}
        }
    }

    private void OnGoBack(object sender, RoutedEventArgs e) => _mainWindow.ShowMainMenu();
}