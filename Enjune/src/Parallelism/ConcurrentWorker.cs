using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Enjune.Logger;
using Enjune.Misc;

namespace Enjune.Parallelism;

[LogParams(logCallingMethod: true)]
public abstract class ConcurrentWorker<TJobKey, TIn, TOut> : AbstractDisposable where TJobKey : IEquatable<TJobKey>
{
    private ulong _generation = 0; // used to track order
    //private readonly BlockingCollection<(TJobKey Key, ulong Generation, TIn Input)> _jobs = new(new ConcurrentQueue<(TJobKey, ulong, TIn)>());
    private readonly ConcurrentDictionary<TJobKey, ulong> 
        _queuedForCancellation = [];
    private readonly ConcurrentQueue<(TJobKey Key, TOut Output)> 
        _done = [];
    private readonly ConcurrentQueue<(TJobKey Key, TIn Input)> 
        _cancelled = [];
    private readonly ConcurrentDictionary<TJobKey, Nothing> 
        _queuedKeys = [];
    private readonly BlockingCollection<(TJobKey Key, ulong Generation, TIn Input)>
        _pendingJobs = new(new ConcurrentQueue<(TJobKey Key, ulong Generation, TIn Input)>());
    
    private SemaphoreSlim? _taskSlots;
    private Task? _dispatcherTask;
    private CancellationTokenSource? _dispatcherCanceller;
    
    #region Public Api
    
    public void Start(int maxParallelWorkers)
    {
        if (HasStarted)
        {
            Logger.Logger.Error(this, "Worker has started already");
            return;
        }

        _taskSlots = new(maxParallelWorkers, maxParallelWorkers);
        _dispatcherCanceller = new CancellationTokenSource();
        _dispatcherTask = Task.Run(async () =>
        {
            try
            {
                var token = _dispatcherCanceller.Token;
                foreach (var job in _pendingJobs.GetConsumingEnumerable(token))
                {
                    await _taskSlots.WaitAsync(token);
                    ThreadPool.QueueUserWorkItem(DoJobWrapper, job);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Logger.Logger.Error(this, $"Error in dispatcher loop: {e}");
            }
        });
    }
    
    public void Stop()
    {
        if (!HasStarted)
        {
            Logger.Logger.Error(this, "Worker has not started yet");
            return;
        }

        _dispatcherCanceller!.Cancel();
        _dispatcherTask!.Wait();
        
        _dispatcherCanceller.Dispose();
        _taskSlots!.Dispose();
        _dispatcherTask = null;
        _dispatcherCanceller = null;
    }
    
    public bool HasStarted => _dispatcherTask != null;
    
    public bool IsEnqueued(TJobKey key) => _queuedKeys.ContainsKey(key);
    
    /// <summary>
    /// Add new job for worker in FIFO order
    /// </summary>
    public void Enqueue(TJobKey key, TIn input)
    {
        if (!HasStarted)
        {
            Logger.Logger.Error(this, "worker hasn't started yet");
            return;
        }
        var gen = Interlocked.Increment(ref _generation);
        _queuedKeys[key] = Nothing.Instance;
        _pendingJobs.Add((key, gen, input));
    }

    private void DoJobWrapper(object? rawJob)
    {
        DoJob(rawJob);
        _taskSlots!.Release();
    }
    
    private void DoJob(object? rawJob)
    {
        var job = ((TJobKey Key, ulong Generation, TIn Input)) rawJob!;
        if (_queuedForCancellation.TryRemove(job.Key, out var cancelGen)) // job was canceled before work
        {
            if (job.Generation < cancelGen) // task appeared before cancel request
            {
                _cancelled.Enqueue((job.Key, job.Input));
                _queuedKeys.Remove(job.Key, out _);
                return;
            }
        }
        // trying to execute job
        try
        {
            var res = Work(job.Input); // working
            _queuedKeys.Remove(job.Key, out _);
            if (_queuedForCancellation.TryRemove(job.Key, out _)) // job was canceled during work
            {
                _cancelled.Enqueue((job.Key, job.Input));
                return;
            }
            // or finally it was not canceled
            _done.Enqueue((job.Key, res));
        }
        catch (OperationCanceledException) // job canceled itself earlier
        {
            _queuedForCancellation.Remove(job.Key, out _); // removing cancel token if had
            _cancelled.Enqueue((job.Key, job.Input)); // enqueuing as canceled
        }
        catch (Exception e)
        {
            Logger.Logger.Error(this, $"Exception during work: \n{e}");
        }   
    }

    /// <summary>
    /// Flags job as canceled. Use TryDequeueCancelled to know when worker stopped doing it
    /// </summary>
    public void EnqueueJobCancel(TJobKey key)
    {
        var gen = Interlocked.Increment(ref _generation);
        _queuedForCancellation[key] = gen;
    }

    /// <summary>
    /// Tries to deque done works if any
    /// </summary>
    public bool TryDequeueDone([MaybeNullWhen(false)] out TJobKey key, [MaybeNullWhen(false)] out TOut output)
    {
        var dequeued = _done.TryDequeue(out var tuple);
        (key, output) = tuple;
        return dequeued;
    }
    
    /// <summary>
    /// Tries to deque canceled works if any
    /// </summary>
    public bool TryDequeueCancelled([MaybeNullWhen(false)] out TJobKey key, [MaybeNullWhen(false)] out TIn input)
    {
        var dequeued = _cancelled.TryDequeue(out var tuple);
        (key, input) = tuple;
        return dequeued;
    }

    #endregion

    #region Private

    protected abstract TOut Work(TIn job);

    protected override void DisposeData()
    {
        _dispatcherCanceller?.Dispose();
        _taskSlots?.Dispose();
        _pendingJobs.Dispose();
    }

    #endregion
}