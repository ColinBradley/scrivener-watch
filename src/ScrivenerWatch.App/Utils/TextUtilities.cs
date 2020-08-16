namespace ScrivenerWatch.App.Utils
{
    internal static class TextUtilities
    {
        public static readonly char[] DiffernceSeparators = new[] { ' ', '\n' };

        public static int GetWordCount(string text)
        {
            return text.Split(DiffernceSeparators).Length;
        }
    }
}
