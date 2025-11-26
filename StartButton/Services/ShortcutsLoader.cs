#nullable enable
using StartButton.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace StartButton.Services;

public class ShortcutsLoader
{
    public StartConfiguration? LoadShortcuts(Form owner, string filename)
    {
        var result = new StartConfiguration();
        string source;

        using (var sr = new StreamReader(filename))
        {
            source = sr.ReadToEnd();
            sr.Close();
        }

        var dom = new XmlDocument();
        dom.LoadXml(source);
        var shortCuts = dom.DocumentElement?.SelectNodes("shortCut");

        if (shortCuts == null)
        {
            MessageBox.Show(
                owner,
                $@"XML document ({filename}) did not contain shortCuts/shortCut.",
                owner.Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            owner.Close();
            return null;
        }

        foreach (XmlElement shortCut in shortCuts)
        {
            var name = shortCut.SelectSingleNode("name")?.InnerText ?? "";
            var warn = (shortCut.Attributes.GetNamedItem("warn")?.Value?.ToLower().Trim() ?? "") == "true";
            var processes = shortCut.SelectSingleNode("processList")?.SelectNodes("process");

            if (processes == null)
                continue;

            var p = (from XmlElement process in processes select process.InnerText).ToList();
            var parsedShortCut = new ShortCut(name, warn);

            foreach (var path in p)
                parsedShortCut.ProcessList.Add(new Process(path));

            result.ShortCuts.Add(parsedShortCut);
        }

        return result;
    }
}