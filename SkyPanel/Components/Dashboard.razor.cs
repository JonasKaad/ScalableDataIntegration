using Microsoft.AspNetCore.Components;

namespace SkyPanel.Components;

public partial class Dashboard : ComponentBase
{
    private string _name = "World";


    public void SetName(string Name)
    {
        _name = Name;
    }

    public void PrintName()
    {
        Console.WriteLine(_name);
    }
}