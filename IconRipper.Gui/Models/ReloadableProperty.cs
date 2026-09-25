using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace IconRipper.Gui.Models;

public sealed class ReloadableProperty<T> where T : class
{
    private CancellationTokenSource? _opCts;

    public async Task LoadProperty(Func<CancellationToken, Task<T>> onLoad, Action<T?>? onResult,
        Action<T?>? onCancelled = null)
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var oldCts = Interlocked.Exchange(ref _opCts, cts);

        await (oldCts?.CancelAsync() ?? Task.CompletedTask);
        oldCts?.Dispose();

        T? result = null;

        try
        {
            token.ThrowIfCancellationRequested();
            result = await onLoad(token);

            token.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!ReferenceEquals(_opCts, cts))
                {
                    onCancelled?.Invoke(result);

                    return;
                }

                onResult?.Invoke(result);
            });
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.InvokeAsync(() => onCancelled?.Invoke(result));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            Interlocked.CompareExchange(ref _opCts, null, cts);
            cts.Dispose();
        }
    }

    public async Task UnloadProperty(Action? onUnload = null)
    {
        var oldCts = Interlocked.Exchange(ref _opCts, null);

        await (oldCts?.CancelAsync() ?? Task.CompletedTask);
        oldCts?.Dispose();

        onUnload?.Invoke();
    }
}