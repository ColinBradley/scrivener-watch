using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ScrivenerWatch.App.Utils
{
    internal static class ScrivenerUtilities
    {
        public static Task<ScivenerEntryBase[]> GetDraftInformation(string filePath)
        {
            return FileUtilities.TryIOTask(async () =>
            {
                using var file = new FileStream(filePath, FileMode.Open);

                var document = await XDocument.LoadAsync(file, LoadOptions.None, default);

                var drafItems = document.Root
                    .Descendants("BinderItem")
                    .FirstOrDefault(e => e.Attribute("Type")?.Value == "DraftFolder")?
                    .Descendants("BinderItem")
                    .Where(e => e.Attribute("Type")?.Value == "Text")
                    .Select(e => new ScivenerEntryBase(e.Attribute("ID").Value, e.Element("Title").Value))
                    .ToArray();

                if (drafItems == null)
                {
                    throw new InvalidDataException("Invalid scrivener file format");
                }

                return drafItems;
            });
        }
    }

    internal record ScivenerEntryBase(
        string Id,
        string Title
    );

    internal record ScivenerEntry(
        string Id,
        string Title,
        string Text,
        int WordCount
    );
}
