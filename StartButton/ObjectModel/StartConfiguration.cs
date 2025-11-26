#nullable enable
using System.Collections.Generic;

namespace StartButton.ObjectModel;

public class StartConfiguration
{
    public List<ShortCut> ShortCuts { get; }

    public StartConfiguration()
    {
        ShortCuts = [];
    }
}