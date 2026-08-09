using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;

namespace UiAddon.Element;

public abstract class UiElement
{
    // base settings
    public readonly NotifyChange<float> GlobalZ;
    public readonly NotifyChange<Rect> LocalAnchor;
    public readonly NotifyChange<Margin> Margin;

    private readonly NotifyChange<Rect> _globalRect = new Rect((0, 0), (500, 500));
    public IReadonlyNotifyChange<Rect> GlobalRect => _globalRect;

    public readonly NotifyChange<bool> LocalVisible = true; // self and children visibility
    public readonly NotifyChange<bool> IsHovered = false;

    private UiElement? _parent;
    private List<UiElement>? _children;
    public readonly List<Model.Entry> Meshes = [];

    protected UiElement(UiElement[] children, Rect localAnchor, Margin margin, float globalZ)
    {
        LocalAnchor = localAnchor;
        Margin = margin;
        GlobalZ = globalZ;
        children.ForEach(AddChild);

        GlobalZ.OnChange += (oldValue, newValue) =>
        {
            var diff = newValue - oldValue;
            foreach (var entry in Meshes)
                entry.Mesh.Offset((0, 0, diff));
        };
        LocalAnchor.OnChange += (_, _) =>
        {
            if (_parent is null)
                Logger.Warn(this, "can not update rect cause parent is null");
            else 
                UpdateGlobalRect(_parent._globalRect);
        };
        Margin.OnChange += (_, _) =>
        {
            if (_parent is null)
                Logger.Warn(this, "can not update rect cause parent is null");
            else 
                UpdateGlobalRect(_parent._globalRect);
        };
        LocalVisible.OnChange += (_, _) => NotifyParentAboutMeshChanges();

        _globalRect.OnChange += (_, newRect) => ForeachChild(ch => ch.UpdateGlobalRect(newRect));
        _globalRect.OnChange += UpdateShape;
    }

    protected abstract void UpdateShape(Rect oldValue, Rect newValue);

    // children
    public void ForeachChild(Action<UiElement> action) => _children?.ForEach(action);
    public void ClearChildren()
    {
        if (_children is null) return;
        _children.ForEach(ch => ch._parent = null);
        _children.Clear();
    }
    public void AddChild(UiElement child)
    {
        if (_children?.Contains(child) ?? false)
        {
            Logger.Warn(this, $"adding child {child} that is already presented in ${_children}");
            return;
        }

        _children ??= new List<UiElement>(1);
        child._parent = this;
        _children.Add(child);
        
        if (child.Meshes.Count > 0)
            NotifyParentAboutMeshChanges();
    }
    public void RemoveChild(UiElement child)
    {
        var removed = _children?.Remove(child) ?? false;
        if (removed)
        {
            child._parent = null;
            if (child.Meshes.Count > 0)
                NotifyParentAboutMeshChanges();
        }
        else
            Logger.Warn(this, $"removing child {child} that is already not in ${_children}");
    }
    // children end
    
    // hover
    public virtual BeingHoveredAction UpdateBeingHovered(BasicInputHandler inputHandler)
    {
        return BeingHoveredAction.BecomeFocused;
    }

    public virtual BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
    {
        return IsHovered ? BeingFocusedAction.ContinueBeing : BeingFocusedAction.StopBeing;
    } 
    
    protected void AddDebugArrowsToMeshes()
    {
        var anchorSize = MathF.Max(10, MathF.Sqrt(_globalRect.Val.Size.X + _globalRect.Val.Size.Y));
        var color = Color.One;
        {
            var minAnchor = Mesh.Triangle((0.5f, 0, 0), (1, 1, 0), (0, 0.5f, 0), TextureQuad.Full);
            minAnchor.Offset((-1, -1, 0));
            minAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            minAnchor.Offset(new Vector3(_globalRect.Val.Min));
            minAnchor.Offset((0, 0, GlobalZ + 5));
            Meshes.Add(new Model.Entry(minAnchor, new Model.PerMesh(color)));
        }
        {
            var maxAnchor = Mesh.Triangle((0f, 0, 0), (1f, 0.5f, 0), (0.5f, 1, 0), TextureQuad.Full);
            maxAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            maxAnchor.Offset(new Vector3(_globalRect.Val.Max));
            maxAnchor.Offset((0, 0, GlobalZ + 5));
            Meshes.Add(new Model.Entry(maxAnchor, new Model.PerMesh(color)));
        }
    }
    
    protected void NotifyParentAboutMeshChanges()
    {
        if (_parent is null)
        {
            Logger.Warn(this, $"can not execute {nameof(NotifyParentAboutMeshChanges)} cause {nameof(_parent)} is null");
            return;
        }
        _parent.OnChildMeshesChanges();
    }

    protected virtual void OnChildMeshesChanges() => NotifyParentAboutMeshChanges(); // pass it up

    public void UpdateGlobalRect(Rect newParentRect)
    {
        var globalAnchor = new Rect(
            newParentRect.Min + newParentRect.Size * LocalAnchor.Val.Min,
            newParentRect.Min + newParentRect.Size * LocalAnchor.Val.Max);

        _globalRect.Val = new Rect(
            (globalAnchor.Min.X + Margin.Val.Left, globalAnchor.Min.Y + Margin.Val.Bottom), 
            (globalAnchor.Max.X - Margin.Val.Right, globalAnchor.Max.Y - Margin.Val.Top));
    }
    
    public enum BeingHoveredAction
    {
        BecomeFocused,
        DoNotBecomeFocused
    }
    public enum BeingFocusedAction
    {
        ContinueBeing,
        StopBeing
    }
}