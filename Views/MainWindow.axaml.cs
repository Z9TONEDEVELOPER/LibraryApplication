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

namespace LibraryApp.Views;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public AppConfig _config = new();
    public AppConfig Config => _config;
    public new event PropertyChangedEventHandler? PropertyChanged;

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
        this.WindowState = WindowState.Maximized;
        DataContext = this;
        InitializeComponent();
        LoadConfig();
        ApplyBackground();
        this.KeyDown += OnKeyDown;
        this.Closing += OnWindowClosing;
        this.ExtendClientAreaToDecorationsHint = false;
        this.SystemDecorations = SystemDecorations.None;
        this.ExtendClientAreaToDecorationsHint = true;
    }
    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        // Отменяем стандартное закрытие
        e.Cancel = true;
        
        // Но если нужно — можно добавить условие для Dev Mode:
        if (_config.AppSettings.EnableDevMode) return;
    }
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Проверяем Ctrl+F12
        if (e.Key == Key.F12 && e.KeyModifiers == KeyModifiers.Control)
        {
            ShowDevModeWindow();
        }
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
                // === Взрослые ресурсы ===
                new() { Category = "Adults", Title = "Сайт ЦБС", Url = "https://kircbs.ru/", Icon = "cbs" },
                new() { Category = "Adults", Title = "Литрес", Url = "http://biblio.litres.ru/", Icon = "litres" },
                new() { Category = "Adults", Title = "ИВИС", Url = "https://eivis.ru/", Icon = "ivis" },
                new() { Category = "Adults", Title = "Университетская библиотека", Url = "http://www.biblioclub.ru/", Icon = "biblioclub" },
                new() { Category = "Adults", Title = "Гребенникон", Url = "http://demo.grebennikon.ru", Icon = "grebennikon" },
                new() { Category = "Adults", Title = "БиблиоРоссика", Url = "http://www.bibliorossica.com/", Icon = "bibliorossica" },
                new() { Category = "Adults", Title = "АртПортал", Url = "https://art.biblioclub.ru/", Icon = "artportal" },
                new() { Category = "Adults", Title = "Интегрум", Url = "https://www.integrum.ru/", Icon = "integrum" },
                new() { Category = "Adults", Title = "КСОБ", Url = "https://spblib.ru/", Icon = "ksub" },

                // === Детские ресурсы ===
                new() { Category = "Kids", Title = "Сайт ЦБС", Url = "https://kircbs.ru/", Icon = "cbs" },
                new() { Category = "Kids", Title = "КСОБ", Url = "https://spblib.ru/", Icon = "ksub" },
                new() { Category = "Kids", Title = "Пушкинская библиотека", Url = "https://www.pushkinlib.spb.ru/", Icon = "pushkin" },
                new() { Category = "Kids", Title = "АртПортал", Url = "https://art.biblioclub.ru/", Icon = "artportal" }
            },
            AllowedPrograms = new Dictionary<string, List<ProgramItem>>
            {
                ["Windows"] = new List<ProgramItem>
                {
                    new() { Name = "АРМ Читатель", Path = @"C:\Program Files (x86)\ARM_Reader\ARMReader.exe", Icon = "arm_reader" },
                    new() { Name = "Компьютер", Path = "explorer.exe", Icon = "explorer" },
                    new() { Name = "Блокнот", Path = "notepad.exe", Icon = "notepad" }
                },
                ["Linux"] = new List<ProgramItem>
                {
                    new() { Name = "АРМ Читатель", Path = "/opt/arm-reader/ArmReader", Icon = "arm_reader" },
                    new() { Name = "Файлы", Path = "xdg-open", Icon = "explorer" },
                    new() { Name = "Текст", Path = "gedit", Icon = "notepad" }
                },
                ["macOS"] = new List<ProgramItem>
                {
                    new() { Name = "Finder", Path = "open .", Icon = "explorer" },
                    new() { Name = "TextEdit", Path = "open -a TextEdit", Icon = "notepad" },
                    new() { Name = "Safari", Path = "open -a Safari", Icon = "safari" }
                }
            }
        };
    }

    public void ApplyBackground()
    {
        string bgFileName = _config.AppSettings.CurrentMode == "Kids"
            ? _config.AppSettings.MainBackgroundKids
            : _config.AppSettings.MainBackground;

        try
        {
            // Путь к файлу: рядом с .exe / Assets / Background / файл.png
            string bgPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Background", bgFileName);
            
            if (!File.Exists(bgPath))
            {
                Console.WriteLine($"[WARN] Фон не найден: {bgPath}");
                return;
            }

            var bitmap = new Bitmap(bgPath);
            this.Background = new ImageBrush { Source = bitmap, Stretch = Stretch.UniformToFill };
        }
        catch (Exception ex)
        {
            this.Background = new SolidColorBrush(Color.FromRgb(248, 244, 237));
            Console.WriteLine($"[ERROR] Не удалось загрузить фон: {ex.Message}");
        }
        try
        {
            string headerPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Background", "cbs_header.png");
            if (File.Exists(headerPath))
            {
                var headerBitmap = new Bitmap(headerPath);
                var headerImage = new Image
                {
                    Source = headerBitmap,
                    Stretch = Stretch.None,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
                };
                HeaderContainer.Child = headerImage;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Не удалось загрузить header: {ex.Message}");
        }
    }

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
            ShowMainMenu();
        }
    }

    public void HideSidebar(object sender, RoutedEventArgs e)
    {
        Sidebar.IsVisible = false;
        SidebarWidth = 0;
    }

    public void ShowDevMode(object sender, RoutedEventArgs e)
    {
        MessageBoxManager.GetMessageBoxStandard(
            "Dev Mode",
            $"Режим: {_config.AppSettings.CurrentMode}\nВерсия: 1.0"
        ).ShowAsync();
    }
    private void ShowDevModeWindow()
    {
        var devWindow = new DevModeWindow(this);
        devWindow.ShowDialog(this);
    }
}