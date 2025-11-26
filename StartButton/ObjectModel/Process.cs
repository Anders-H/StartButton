#nullable enable
namespace StartButton.ObjectModel;

public class Process
{
    public string ProcessPath { get; }

    public Process(string processPath)
    {
        ProcessPath = processPath;
    }
}