#nullable enable
using System.Collections.Generic;

namespace StartButton.ObjectModel;

public class ShortCut
{
    public string Name { get; }
    public bool Warn { get; }
    public List<Process> ProcessList { get; }

    public ShortCut(string name, bool warn)
    {
        Name = name;
        Warn = warn;
        ProcessList = [];
    }
}