using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Enjune.Logger;
using Enjune.Misc;

namespace Enjune.Parallelism;

[LogParams(logCallingMethod: true)]
public abstract class ConcurrentWorker<TJobKey, TIn, TOut> : AbstractDisposable where TJobKey : IEquatable<TJobKey>
{
    private ulong _generation = 0; // used to track order
    private readonly BlockingCollection<(TJobKey Key, ulong Generation, TIn Input)> _jobs = new(new ConcurrentQueue<(TJobKey, ulong, TIn)>());
    private readonly ConcurrentDictionary<TJobKey, ulong> _queuedForCancellation = [];
    private readonly ConcurrentQueue<(TJobKey Key, TOut Output)> _done = [];
    private readonly ConcurrentQueue<(TJobKey Key, TIn Input)> _cancelled = [];
    private Thread? _workingThread;
    private CancellationTokenSource? _cancelWorkToken;

    #region Public Api

    public bool HasStarted => _workingThread != null;
    
    /// <summary>
    /// Starts worker in separate thread
    /// </summary>
    public void Start(string threadName)
    {
        if (HasStarted)
        {
            Logger.Logger.Error(this, "Thread is already running");
            return;
        }

        _cancelWorkToken = new CancellationTokenSource();
        _workingThread = new Thread(() =>
        {
            var cancelToken = _cancelWorkToken.Token;
            try
            {
                foreach (var job in _jobs.GetConsumingEnumerable(cancelToken))
                {
                    try
                    {
                        if (_queuedForCancellation.TryRemove(job.Key, out var cancelGen)) // job was canceled before work
                        {
                            if (job.Generation < cancelGen) // task appeared before cancel request
                            {
                                _cancelled.Enqueue((job.Key, job.Input));
                                continue;
                            }
                        }
                        var res = Work(job.Input); // working
                        if (_queuedForCancellation.TryRemove(job.Key, out _)) // job was canceled during work
                        {
                            _cancelled.Enqueue((job.Key, job.Input));
                            continue;
                        } 
                        // or finally it was not canceled
                        _done.Enqueue((job.Key, res));
                    }
                    catch (OperationCanceledException){}
                    catch (Exception e)
                    {
                        Logger.Logger.Error(this, $"Exception during work: \n{e}");
                    }
                }
            }
            catch (OperationCanceledException threadCancelled){}
        });
        _workingThread.Name = threadName;
        _workingThread.IsBackground = true;
        _workingThread.Start();
        Logger.Logger.Info(this, $"Thread {_workingThread.Name} started");
    }

    public void Stop()
    {
        if (_workingThread is null)
        {
            Logger.Logger.Error(this, "Thread is not running");
            return;
        }
        
        _cancelWorkToken!.Cancel();
        
        _workingThread.Join();
        Logger.Logger.Info(this, $"Thread {_workingThread.Name} stopped");
        _workingThread = null;
        
        _cancelWorkToken.Dispose();
        _cancelWorkToken = null;
    }

    /// <summary>
    /// Add new job for worker in FIFO order
    /// </summary>
    public void Enqueue(TJobKey key, TIn input)
    {
        if (_workingThread is null)
        {
            Logger.Logger.Error(this, "Start worker first");
            return;
        }
        var gen = Interlocked.Increment(ref _generation);
        _jobs.Add((key, gen, input));
    }

    /// <summary>
    /// Flags job as canceled. Use TryDequeueCancelled to know when worker stopped doing it
    /// </summary>
    public void EnqueueJobCancel(TJobKey key)
    {
        if (_workingThread is null)
        {
            Logger.Logger.Error(this, "Start worker first");
            return;
        }

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
        if (_workingThread is not null) 
            Stop();
        _jobs.Dispose();
    }

    #endregion
}