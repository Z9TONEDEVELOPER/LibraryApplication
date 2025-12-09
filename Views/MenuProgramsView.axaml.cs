using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

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
                if (OperatingSystem.IsMacOS())
                {
                    // Для macOS используем /bin/bash для выполнения команд
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
                    // Для Windows и Linux
                    Process.Start(rawPath);
                }
            }
            catch 
            {
                // Можно добавить MessageBox, но для MVP оставим без ошибок
            }
        }
    }

    private void OnGoBack(object sender, RoutedEventArgs e) => _mainWindow.ShowMainMenu();
}