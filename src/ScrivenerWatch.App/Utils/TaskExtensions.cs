using System.Threading.Tasks;

namespace ScrivenerWatch.App.Utils
{
    internal static class TaskExtensions
    {
        public static void IgnoreAwait(this Task _)
        {
            // Nothing to do
        }
    }
}
