using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitaiImageDownloader.Util;
public class LoopHelper
{

    public static void Loop<T>(bool parallelMode, IEnumerable<T> items, Func<T, bool> action)
    {
        if (parallelMode)
        {
            try
            {
                Parallel.ForEach(items, i =>
                {
                    if (!action.Invoke(i))
                        throw new OperationCanceledException();
                });
            }
            catch (OperationCanceledException)
            {
                // stopped by flow control
            }
        }
        else
        {
            foreach (var item in items)
            {
                if (!action.Invoke(item))
                {
                    break;
                }
            }
        }
    }

    public static async Task LoopAsync(bool parallelMode, CancellationTokenSource cts, int n, Func<int, ValueTask<bool>> action)
    {
        ParallelOptions options = new ParallelOptions { CancellationToken = cts.Token };
        if (parallelMode)
        {
            try
            {
                await Parallel.ForAsync(0, n, options, async (i, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    await action.Invoke(i);
                });
            }
            catch (OperationCanceledException)
            {
                // stopped by ct
            }
        }
        else
        {
            for (var i = 0; i < n; i++)
            {
                if (!await action.Invoke(i))
                {
                    break;
                }
            }
        }
    }
}
