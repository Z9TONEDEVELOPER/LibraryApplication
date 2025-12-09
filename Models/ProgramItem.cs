namespace LibraryApp.Models;

public class ProgramItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Icon { get; set; } = "default";

    
    public string IconPath => $"Assets/Icons/{Icon}.png";
}