using System.Collections.Generic;

namespace StartButton
{
    public class StartConfiguration
    {
        public List<ShortCut> ShortCuts { get; }

        public StartConfiguration()
        {
            ShortCuts = new List<ShortCut>();
        }
    }

    public class ShortCut
    {
        public string Name { get; }
        public bool Warn { get; }
        public List<Process> ProcessList { get; }

        public ShortCut(string name, bool warn)
        {
            Name = name;
            Warn = warn;
            ProcessList = new List<Process>();
        }
    }

    public class Process
    {
        public string ProcessPath { get; }

        public Process(string processPath)
        {
            ProcessPath = processPath;
        }
    }
}