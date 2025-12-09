namespace LibraryApp.Models;

public class ResourceItem
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "default";

    
    public string IconPath => $"Assets/Icons/{Icon}.png";
}