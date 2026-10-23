using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Enjune.Attribute;
using Enjune.Misc;

namespace Minecraft2.Misc;

[LogParams(logCallingMethod: true)]
public abstract class ConcurrentWorker<TJobKey, TIn, TOut> : AbstractDisposable where TIn : notnull where TJobKey : unmanaged
{
    private readonly BlockingCollection<(TJobKey Key, TIn Input)> _jobs = new(new ConcurrentQueue<(TJobKey, TIn)>());
    private readonly ConcurrentDictionary<TJobKey, Nothing> _cancelledJobs = [];
    private readonly ConcurrentQueue<(TJobKey Key, TOut Output)> _done = [];
    private Thread? _workingThread;
    private CancellationTokenSource? _cancelWork;

    #region Public Api
    
    /// <summary>
    /// Starts worker in separate thread
    /// </summary>
    /// <param name="threadName"></param>
    public void Start(string threadName)
    {
        if (_workingThread is not null)
        {
            Logger.Error(this, "Thread is already running");
            return;
        }

        _cancelWork = new CancellationTokenSource();
        _workingThread = new Thread(() =>
        {
            var cancelToken = _cancelWork.Token;
            try
            {
                foreach (var job in _jobs.GetConsumingEnumerable(cancelToken))
                {
                    try
                    {
                        if (_cancelledJobs.TryRemove(job.Key, out _)) // job was canceled before work
                            continue;
                        var res = Work(job.Input);
                        if (_cancelledJobs.TryRemove(job.Key, out _)) // job was canceled after work
                            continue;
                        _done.Enqueue((job.Key, res));
                    }
                    catch (OperationCanceledException){}
                    catch (Exception e)
                    {
                        Logger.Error(this, $"Exception during work: \n{e}");
                    }
                }
            }
            catch (OperationCanceledException threadCancelled){}
        });
        _workingThread.Name = threadName;
        _workingThread.IsBackground = true;
        _workingThread.Start();
        Logger.Info(this, $"Thread {_workingThread.Name} started");
    }

    public void Stop()
    {
        if (_workingThread is null)
        {
            Logger.Error(this, "Thread is not running");
            return;
        }
        
        _cancelWork!.Cancel();
        
        _workingThread.Join();
        Logger.Info(this, $"Thread {_workingThread.Name} stopped");
        _workingThread = null;
        
        _cancelWork.Dispose();
        _cancelWork = null;
        
        _cancelledJobs.Clear();
    }

    /// <summary>
    /// Add new job for worker in FIFO order
    /// </summary>
    /// <param name="key"></param>
    /// <param name="input"></param>
    public void Enqueue(TJobKey key, TIn input)
    {
        if (_workingThread is null)
        {
            Logger.Error(this, "Start worker first");
            return;
        }
        _jobs.Add((key, input));
    }

    /// <summary>
    /// Cancels queued job 
    /// </summary>
    /// <param name="key"></param>
    public void CancelJob(TJobKey key) => _cancelledJobs.TryAdd(key, Nothing.Instance);

    /// <summary>
    /// Tries to deque done works if any
    /// </summary>
    public bool TryDequeue(out TJobKey key, out TOut output)
    {
        var dequeued = _done.TryDequeue(out var tuple);
        (key, output) = tuple;
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