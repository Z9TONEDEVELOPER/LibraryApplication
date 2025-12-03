using System.Collections.Generic;
namespace LibraryApp.Models;

public class Header
{
    public string Title { get; set; } = string.Empty;
    public List<HeaderItem> MenuItems { get; set; } = new();
}