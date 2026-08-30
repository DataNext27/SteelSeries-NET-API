using Microsoft.Extensions.Logging;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>
/// Runs a refresh callback with two entry points: a debounced schedule (invalidation
/// bursts collapse into a single refresh) and an immediate, awaited run (polling tick).
/// Both paths are serialized by an internal lock.
/// </summary>
internal sealed class DebouncedRefresher : IDisposable
{
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(250);

    private readonly string _name;
    private readonly Func<CancellationToken, Task> _refresh;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private int _version;

    /// <summary>Creates a refresher.</summary>
    /// <param name="name">A short name used in log messages (e.g. "redirections").</param>
    /// <param name="refresh">The refresh work. Must not manage its own locking.</param>
    /// <param name="logger">The listener's logger.</param>
    internal DebouncedRefresher(string name, Func<CancellationToken, Task> refresh, ILogger logger)
    {
        _name = name;
        _refresh = refresh;
        _logger = logger;
    }

    /// <summary>
    /// Schedules a refresh after the debounce delay. Each call supersedes the previous
    /// one, so only the last invalidation of a burst actually refreshes.
    /// </summary>
    internal void Schedule(CancellationToken ct)
    {
        int version = Interlocked.Increment(ref _version);
        _logger.LogDebug("{Name} refresh #{Version} scheduled", _name, version);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceDelay, ct).ConfigureAwait(false);
                if (version != _version)
                {
                    _logger.LogDebug("{Name} refresh #{Version} superseded", _name, version);
                    return;
                }

                await RunNowAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Listener stopping: expected, stay silent.
            }
            catch (Exception ex)
            {
                // Includes HTTP timeouts (TaskCanceledException with a non-cancelled token).
                _logger.LogWarning(ex, "{Name} refresh #{Version} failed", _name, version);
            }
        }, ct);
    }

    /// <summary>Runs the refresh immediately, serialized with any scheduled refresh.</summary>
    internal async Task RunNowAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try { await _refresh(ct).ConfigureAwait(false); }
        finally { _lock.Release(); }
    }

    /// <inheritdoc />
    public void Dispose() => _lock.Dispose();
}