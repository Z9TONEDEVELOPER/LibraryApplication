using System.Diagnostics;
using System.IO;
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

public partial class MainWindow : Window
{
    private AppConfig _config = new();
    public AppConfig Config => _config;

    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();
        LoadConfig();
        BindResources();
        BindPrograms();
        ProgramItemsControl.ItemsSource = GetCurrentPrograms();
        ApplyBackground();
        DevTrigger.PointerPressed += (s, e) => ShowDevInfo();
    }


    private void LoadConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        try
        {
            // Если файла нет — создаём шаблон
            if (!File.Exists(configPath))
            {
                var defaultConfig = CreateDefaultConfig();
                var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json, Encoding.UTF8);
            }

            // Читаем и парсим
            var jsonText = File.ReadAllText(configPath, Encoding.UTF8);
            var config = JsonSerializer.Deserialize<AppConfig>(jsonText);

            if (config != null)
            {
                _config = config;
                return;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Нет прав на запись/чтение — работаем с дефолтным конфигом
        }
        catch (JsonException ex)
        {
            // Битый JSON — предупреждаем и используем дефолт
            MessageBoxManager.GetMessageBoxStandard(
                "Ошибка конфигурации",
                $"Файл appsettings.json содержит ошибки:\n{ex.Message}\n\nБудет использована конфигурация по умолчанию."
            ).ShowAsync();
        }
        catch (Exception ex)
        {
            // Любая другая ошибка
            MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                $"Не удалось загрузить настройки:\n{ex.Message}\n\nБудет использована конфигурация по умолчанию."
            ).ShowAsync();
        }

        // Если всё сломалось — используем встроенный шаблон
        _config = CreateDefaultConfig();
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
            Header = new Header
            {
                Title = "САНКТ-ПЕТЕРБУРГСКОЕ ГОСУДАРСТВЕННОЕ БЮДЖЕТНОЕ УЧРЕЖДЕНИЕ КУЛЬТУРЫ \"ЦЕНТРАЛИЗОВАННАЯ БИБЛИОТЕЧНАЯ СИСТЕМА КИРОВСКОГО РАЙОНА\"",
                MenuItems = new List<HeaderItem>
                {
                    new() { Name = "О ЦБС", Url = "https://kircbs.ru/about" },
                    new() { Name = "Услуги", Url = "https://kircbs.ru/services" },
                    new() { Name = "Новости", Url = "https://kircbs.ru/news" },
                    new() { Name = "Афиша", Url = "https://eivis.ru/" },
                    new() { Name = "Анонсы", Url = "https://spblib.ru/" },
                    new() { Name = "Электронный каталог", Url = "https://spblib.ru/catalog" },
                    new() { Name = "Виртуальный читальный зал", Url = "https://spblib.ru/virtual" },
                    new() { Name = "Советуем почитать", Url = "https://litres.ru/recommendations" }
                }
            },
            Resources = new List<ResourceItem>
            {
                // === Взрослые ресурсы ===
                new() { Category = "Adults", Title = "Сайт ЦБС", Url = "https://kircbs.ru/" },
                new() { Category = "Adults", Title = "Литрес", Url = "http://biblio.litres.ru/" },
                new() { Category = "Adults", Title = "ИВИС", Url = "https://eivis.ru/" },
                new() { Category = "Adults", Title = "Университетская библиотека", Url = "http://www.biblioclub.ru/" },
                new() { Category = "Adults", Title = "Гребенникон", Url = "http://demo.grebennikon.ru" },
                new() { Category = "Adults", Title = "БиблиоРоссика", Url = "http://www.bibliorossica.com/" },
                new() { Category = "Adults", Title = "АртПортал", Url = "https://art.biblioclub.ru/" },
                new() { Category = "Adults", Title = "Интегрум", Url = "https://www.integrum.ru/" },
                new() { Category = "Adults", Title = "КСОБ", Url = "https://spblib.ru/" },

                // === Детские ресурсы ===
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
                    new() { Name = "Файловый менеджер", Path = "xdg-open" },
                    new() { Name = "Текстовый редактор", Path = "gedit" }
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
    private void BindResources()
    {
        var filtered = _config.Resources
            .Where(r => r.Category == _config.AppSettings.CurrentMode)
            .ToList();

        ResourceItemsControl.ItemsSource = filtered;
        ApplyBackground(); 
    }

    private void BindPrograms()
    {
        
        var currentPrograms = GetCurrentPrograms(); // ← Получаем список для текущей ОС
        ProgramItemsControl.ItemsSource = currentPrograms; 
    }

    private async void OnResourceClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string url)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Не удалось открыть: {ex.Message}").ShowAsync();
            }
        }
    }

    private void OnProgramClick(object sender, RoutedEventArgs e)
{
    if (sender is Button btn && btn.Tag is string rawPath)
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                // На macOS запускаем через терминал
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
                // Windows / Linux
                Process.Start(new ProcessStartInfo
                {
                    FileName = rawPath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard(
                "Ошибка запуска",
                $"Не удалось запустить программу:\n{ex.Message}"
            ).ShowAsync();
        }
    }
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
    private void ApplyBackground()
    {
        string bgFileName = _config.AppSettings.CurrentMode == "Kids"
            ? _config.AppSettings.MainBackgroundKids
            : _config.AppSettings.MainBackground;

        try
        {
            // Используем правильный путь через AssetLoader
            var uri = new Uri($"avares://LibraryApp/Assets/Background/{bgFileName}");
            using var stream = AssetLoader.Open(uri);
            var bitmap = new Bitmap(stream);

            this.Background = new ImageBrush
            {
                Source = bitmap,
                Stretch = Stretch.UniformToFill
            };
        }
        catch (Exception ex)
        {
            // Запасной фон, если что-то пошло не так
            this.Background = new SolidColorBrush(Color.FromRgb(248, 244, 237)); // #282724ff
            System.Diagnostics.Debug.WriteLine($"[Warning] Не удалось загрузить фон: {ex.Message}");
        }
    }
    private void ShowDevInfo()
    {
        if (!_config.AppSettings.EnableDevMode) return;

        var version = "v1.0";
        var info = $"Читательский киоск {version}\n" +
                   $"Режим: {_config.AppSettings.CurrentMode}\n" +
                   $"Разработчик: Савин М.А.\n" +
                   $"Год: 2025";

        MessageBoxManager.GetMessageBoxStandard("Информация", info).ShowAsync();
    }
}