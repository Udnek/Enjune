using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Enjune.Attribute;
using Enjune.Misc;

namespace Minecraft2.Misc;

[LogParams(logCallingMethod: true)]
public abstract class ConcurrentWorker<TIn, TOut> : AbstractDisposable
{
    private readonly BlockingCollection<TIn> _jobs = new(new ConcurrentQueue<TIn>());
    private readonly ConcurrentQueue<(TIn, TOut)> _done = [];
    private Thread? _workingThread;
    private CancellationTokenSource? _cancelWork;

    #region Public Api
    
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
                        var res = Work(job);
                        _done.Enqueue((job, res));
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
        _workingThread = null;
        
        _cancelWork.Dispose();
        _cancelWork = null;
    }

    public void Enqueue(TIn input)
    {
        if (_workingThread is null)
        {
            Logger.Error(this, "Start worker first");
            return;
        }
        _jobs.Add(input);
    }

    public bool TryTake(out (TIn input, TOut output) result)
        => _done.TryDequeue(out result);
    
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