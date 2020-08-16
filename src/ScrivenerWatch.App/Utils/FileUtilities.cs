using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrivenerWatch.App.Utils
{
    internal static class FileUtilities
    {
        public static async Task TryIOTask(Action action)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    action();
                    return;
                }
                catch (IOException)
                {
                    if (attempt == 4)
                        throw;

                    await Task.Delay(attempt * 200);
                }
            }
        }

        public static async Task<T> TryIOTask<T>(Func<Task<T>> action)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (IOException)
                {
                    if (attempt == 4)
                        throw;

                    await Task.Delay(attempt * 200);
                }
            }
        }
    }
}
