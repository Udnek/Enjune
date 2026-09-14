using Enjune.Attribute;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Element;

namespace UiAddon.Display.Abstraction;

[LogParams(logCallingMethod: true)]
public abstract class AbstractUiDisplay<TParent> : AbstractDisposable, IUiDisplay where TParent : IUiElement
{
    public IList<Model.Entry> Meshes => _meshes;
    public required TParent Parent { get; init; }
    public float ZOffset = 0;
    
    protected float Z => Parent.GlobalZ + ZOffset;
    
    private readonly ObservableList<Model.Entry> _meshes = [];
    private readonly List<Unsubscriber> _unsubscribers = [];

    protected AbstractUiDisplay()
    {
    }

    /// <summary>
    /// Any subscription to parent's changes should be done via this method to avoid memory leaks
    /// </summary>
    /// <param name="parentProperty"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    protected void SubscribeToParent<T>(IReadonlyObservableValue<T> parentProperty, ChangeEvent<T> action)
        => _unsubscribers.Add(parentProperty.Observe(action));

    /// <summary>
    /// Any subscription to parent's changes should be done via this method to avoid memory leaks
    /// </summary>
    /// <param name="parentProperty"></param>
    /// <param name="onAdded"></param>
    /// <param name="onRemoved"></param>
    /// <typeparam name="T"></typeparam>
    protected void SubscribeToParent<T>(IObservableReadonlyList<T> parentProperty, ListModificationAction<T> onAdded, ListModificationAction<T> onRemoved)
    {
        _unsubscribers.Add(parentProperty.AfterItemAdded(onAdded));
        _unsubscribers.Add(parentProperty.AfterItemRemoved(onRemoved));
    }

    protected abstract void OnRectChange(Rect oldRect, Rect rect);
    
    /// <summary>
    /// base should be called and be called at the end, so OnRectChange trigger after all subscriptions
    /// </summary>
    public virtual void Initialize()
    {
        _meshes.AfterItemAddedAsOwner((_, _) => Parent.MeshesChanged = true);
        _meshes.AfterItemRemovedAsOwner((_, _) => Parent.MeshesChanged = true);
        SubscribeToParent(Parent.Rect, OnRectChange);
        OnRectChange(new Rect(), Parent.Rect.Val); // initial call
    }
    
    public void UnsubscribeFromParent() => Dispose();

    protected override void DisposeData()
    {
        Logger.Info(this, $"Unsubscribing {_unsubscribers.Count} delegates");
        _unsubscribers.ForEach(u => u.Unsubscribe());
        _unsubscribers.Clear();
    }
}