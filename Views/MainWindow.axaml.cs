using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using LibraryApp.Models;
using System.Collections.Generic;
using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Linq;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using System.Text;
using Avalonia.Data.Converters;  
using System.Globalization;       


namespace LibraryApp.Views;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private AppConfig _config = new();
    public AppConfig Config => _config;
    public new event PropertyChangedEventHandler? PropertyChanged;
    private bool _isSidebarOpen = false;
    private object? _currentSidebarView;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private double _sidebarWidth = 0;
    public double SidebarWidth
    {
        get => _sidebarWidth;
        set
        {
            _sidebarWidth = value;
            OnPropertyChanged(nameof(SidebarWidth));
        }
    }

    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();
        LoadConfig();
        ApplyBackground();
        
    }

    private void LoadConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        try
        {
            if (!File.Exists(configPath))
            {
                var defaultConfig = CreateDefaultConfig();
                var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json, Encoding.UTF8);
            }

            var jsonText = File.ReadAllText(configPath, Encoding.UTF8);
            var config = JsonSerializer.Deserialize<AppConfig>(jsonText);

            if (config != null)
            {
                _config = config;
                Console.WriteLine("[DEBUG] Конфиг загружен успешно");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            _config = CreateDefaultConfig();
        }
    }

    private AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            AppSettings = new AppSettings
            {
                CurrentMode = "Adults",
                EnableDevMode = false,
                AutoStart = true,
                HeaderBackground = "cbs_header.png",
                MainBackground = "cbs_main.png",
                MainBackgroundKids = "cbs_main_kids.png"
            },
            Resources = new List<ResourceItem>
            {
                new() { Category = "Adults", Title = "Сайт ЦБС", Url = "https://kircbs.ru/" },
                new() { Category = "Adults", Title = "Литрес", Url = "http://biblio.litres.ru/" },
                new() { Category = "Adults", Title = "ИВИС", Url = "https://eivis.ru/" },
                new() { Category = "Adults", Title = "Университетская библиотека", Url = "http://www.biblioclub.ru/" },
                new() { Category = "Adults", Title = "Гребенникон", Url = "http://demo.grebennikon.ru" },
                new() { Category = "Adults", Title = "БиблиоРоссика", Url = "http://www.bibliorossica.com/" },
                new() { Category = "Adults", Title = "АртПортал", Url = "https://art.biblioclub.ru/" },
                new() { Category = "Adults", Title = "Интегрум", Url = "https://www.integrum.ru/" },
                new() { Category = "Adults", Title = "КСОБ", Url = "https://spblib.ru/" },

                new() { Category = "Kids", Title = "Сайт ЦБС", Url = "https://kircbs.ru/" },
                new() { Category = "Kids", Title = "КСОБ", Url = "https://spblib.ru/" },
                new() { Category = "Kids", Title = "Пушкинская библиотека", Url = "https://www.pushkinlib.spb.ru/" },
                new() { Category = "Kids", Title = "АртПортал", Url = "https://art.biblioclub.ru/" }
            },
            AllowedPrograms = new Dictionary<string, List<ProgramItem>>
            {
                ["Windows"] = new List<ProgramItem>
                {
                    new() { Name = "АРМ Читатель", Path = @"C:\Program Files (x86)\ARM_Reader\ARMReader.exe" },
                    new() { Name = "Компьютер", Path = "explorer.exe" },
                    new() { Name = "Блокнот", Path = "notepad.exe" }
                },
                ["Linux"] = new List<ProgramItem>
                {
                    new() { Name = "АРМ Читатель", Path = "/opt/arm-reader/ArmReader" },
                    new() { Name = "Файлы", Path = "xdg-open" },
                    new() { Name = "Текст", Path = "gedit" }
                },
                ["macOS"] = new List<ProgramItem>
                {
                    new() { Name = "Finder", Path = "open" },
                    new() { Name = "TextEdit", Path = "open -a TextEdit" },
                    new() { Name = "Safari", Path = "open -a Safari" }
                }
            }
        };
    }

    private void ApplyBackground()
    {
        string bgFileName = _config.AppSettings.CurrentMode == "Kids"
            ? _config.AppSettings.MainBackgroundKids
            : _config.AppSettings.MainBackground;

        try
        {
            var uri = new Uri($"avares://LibraryApp/Assets/Background/{bgFileName}");
            using var stream = AssetLoader.Open(uri);
            var bitmap = new Bitmap(stream);
            this.Background = new ImageBrush { Source = bitmap, Stretch = Stretch.UniformToFill };
        }
        catch (Exception ex)
        {
            this.Background = new SolidColorBrush(Color.FromRgb(248, 244, 237));
            Console.WriteLine($"[Warning] Не удалось загрузить фон: {ex.Message}");
        }
    }

    // === НОВЫЕ ОБРАБОТЧИКИ ===

    private async void OpenCbsSite(object sender, RoutedEventArgs e)
    {
        try
        {
            await Launcher.LaunchUriAsync(new Uri("https://kircbs.ru/"));
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Не удалось открыть сайт ЦБС:\n{ex.Message}").ShowAsync();
        }
    }

    public void ShowMainMenu()
    {
        SidebarContent.Content = new MenuMainView(this);
    }

    public void ShowResourcesMenu()
    {
        var filtered = _config.Resources
            .Where(r => r.Category == _config.AppSettings.CurrentMode)
            .ToList();
        
        SidebarContent.Content = new MenuResourcesView(filtered, this);
    }

    public void ShowProgramsMenu()
    {
        var programs = GetCurrentPrograms();
        SidebarContent.Content = new MenuProgramsView(programs, this);
    }

    private List<ProgramItem> GetCurrentPrograms()
    {
        if (OperatingSystem.IsWindows())
        {
            _config.AllowedPrograms.TryGetValue("Windows", out var list);
            return list ?? new List<ProgramItem>();
        }
        else if (OperatingSystem.IsLinux())
        {
            _config.AllowedPrograms.TryGetValue("Linux", out var list);
            return list ?? new List<ProgramItem>();
        }
        else if (OperatingSystem.IsMacOS())
        {
            _config.AllowedPrograms.TryGetValue("macOS", out var list);
            return list ?? new List<ProgramItem>();
        }
        return new List<ProgramItem>();
    }
    private void ToggleSidebar(object sender, RoutedEventArgs e)
    {
        SidebarWidth = SidebarWidth == 0 ? 280 : 0;
        Sidebar.IsVisible = SidebarWidth > 0;
        
        if (Sidebar.IsVisible && SidebarContent.Content == null)
        {
            ShowMainMenu(); // При первом открытии — главное меню
        }
    }

    private void ShowMainMenuButtonClick(object sender, RoutedEventArgs e) => ShowMainMenu();

    public void HideSidebar(object sender, RoutedEventArgs e)
    {
        SidebarWidth = 0;
        Sidebar.IsVisible = false;
    }

    public void ShowDevMode(object sender, RoutedEventArgs e)
    {
        // Пока просто показываем информацию
        MessageBoxManager.GetMessageBoxStandard(
            "Dev Mode",
            $"Режим: {_config.AppSettings.CurrentMode}\nВерсия: 1.0"
        ).ShowAsync();
    }
}