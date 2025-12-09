using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Views;
using LibraryApp.Models;
namespace LibraryApp.Views;

public partial class MenuMainView : UserControl
{
    private readonly MainWindow _mainWindow;

    public MenuMainView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private void OnShowResources(object sender, RoutedEventArgs e) => _mainWindow.ShowResourcesMenu();
    private void OnShowPrograms(object sender, RoutedEventArgs e) => _mainWindow.ShowProgramsMenu();
    private void OnGoBack(object sender, RoutedEventArgs e) => _mainWindow.HideSidebar(null, null);
    private void OnShowDevMode(object sender, RoutedEventArgs e) => _mainWindow.ShowDevMode(null, null);
}