#nullable enable
using System;
using System.IO;

namespace StartButton.Services;

public class ConfigurationFileService
{
    public string GetSuggestedFilename()
    {
        var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StartButtonUserDataFolder");

        var folder = new DirectoryInfo(folderPath);

        if (!folder.Exists)
            folder.Create();

        return Path.Combine(folderPath, "StartButtonConfiguration.xml");
    }

    public DirectoryInfo? TryGetExecutingDirectory()
    {
        var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        var exe = new FileInfo(executingAssembly.Location);
        return exe.Directory;
    }
}