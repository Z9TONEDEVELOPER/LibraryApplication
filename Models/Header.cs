using System.Collections.Generic;
namespace LibraryApp.Models;

public class Header
{
    public List<HeaderItem> MenuItems { get; set; } = new();
}