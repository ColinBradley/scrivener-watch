using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrivenerWatch.App.Utils
{
    internal static class RichTextUtilities
    {
        public static Task<string> ReadFromFile(string path)
            => Task.Run(async () =>
                {
                    using var richTextBox = new RichTextBox();
                    
                    await FileUtilities.TryIOTask(
                        () => richTextBox.LoadFile(path));

                    return richTextBox.Text;
                });
    }
}
