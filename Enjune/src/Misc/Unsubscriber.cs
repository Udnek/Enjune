namespace Enjune.Misc;

/// <summary>
/// Used to avoid memory leaks when working with events
/// </summary>
/// <param name="unsubscribe"></param>
public sealed class Unsubscriber(Action unsubscribe) : AbstractDisposable
{
    public void Unsubscribe() => Dispose();
    
    protected override void DisposeData() => unsubscribe();
}