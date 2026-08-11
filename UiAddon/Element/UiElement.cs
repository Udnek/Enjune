using Enjune.Graphic;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;

namespace UiAddon.Element;

public abstract class UiElement
{
    #region Public
    public Rect GlobalRect => _globalRect;
    
    public readonly NotifyChange<float> GlobalZ;
    public readonly NotifyChange<Rect> LocalAnchor;
    public readonly NotifyChange<Margin> Margin;
    public readonly NotifyChange<bool> LocalVisible = true; // self and children visibility
    public readonly NotifyChange<bool> IsHovered = false;
    
    public IList<UiElement> Children => _children;
    public IList<Model.Entry> Meshes => _meshes;
    #endregion
    
    private readonly NotifyChange<Rect> _globalRect = new Rect((0, 0), (500, 500));
    private UiElement? _parent;
    private readonly NotifyChangeList<UiElement> _children;
    private readonly NotifyChangeList<Model.Entry> _meshes = [];

    protected UiElement(UiElement[] children, Rect localAnchor, Margin margin, float globalZ)
    {
        LocalAnchor = localAnchor;
        Margin = margin;
        GlobalZ = globalZ;

        #region Children
        _children = new NotifyChangeList<UiElement>(children.Length);
        _children.AfterElementAdded += child =>
        {
            child._parent = this;
            if (child.Meshes.Count > 0)
                NotifyParentAboutMeshChanges();
        };
        _children.AfterElementRemoved += child =>
        {
            child._parent = null;
            if (child.Meshes.Count > 0)
                NotifyParentAboutMeshChanges();
        };
        children.ForEach(ch => _children.Add(ch));
        #endregion
        
        #region Meshes
        _meshes.AfterElementAdded += _ => NotifyParentAboutMeshChanges();
        _meshes.AfterElementRemoved += _ => NotifyParentAboutMeshChanges();
        #endregion
        
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


        _globalRect.OnChange += (_, newRect) => _children.ForEach(ch => ch.UpdateGlobalRect(newRect));
        _globalRect.OnChange += UpdateShape;
    }

    protected abstract void UpdateShape(Rect oldValue, Rect newValue);

    // debug purpose
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
    
    private void NotifyParentAboutMeshChanges()
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

    #region Hovering and Focusing

    public virtual BeingHoveredAction UpdateBeingHovered(BasicInputHandler inputHandler)
        => inputHandler.IsJustPressed(KeyCode.LeftMouseButton)
            ? BeingHoveredAction.BecomeFocused
            : BeingHoveredAction.DoNotBecomeFocused;

    public virtual BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
        => IsHovered ? BeingFocusedAction.ContinueBeing : BeingFocusedAction.StopBeing;

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
    
    #endregion
}