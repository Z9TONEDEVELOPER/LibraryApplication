using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace LibraryApp.Models;

public class AppConfig
{
    public AppSettings AppSettings { get; set; } = new();
    public List<ResourceItem> Resources { get; set; } = new();
    public Dictionary<string, List<ProgramItem>> AllowedPrograms { get; set; } = new();
}

public class AppSettings
{
    public string CurrentMode { get; set; } = "Adults";
    public bool EnableDevMode { get; set; } = false;
    public bool AutoStart { get; set; } = true;
}