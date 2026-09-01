using System.Collections.ObjectModel;
using Enjune.Attribute;
using Enjune.Graphic;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;
using UiAddon.Display;

namespace UiAddon.Element;

[LogParams(logCallingMethod: true)]
public abstract class UiElement
{
    #region Public
    public Rect GlobalRect => _globalRect;
    
    public readonly ObservableValue<float> GlobalZ;
    public readonly ObservableValue<Rect> LocalAnchor;
    public readonly ObservableValue<Margin> Margin;
    public readonly ObservableValue<bool> LocalVisible = true; // self and children visibility
    public readonly ObservableValue<bool> IsHovered = false;
    
    public IList<UiElement> Children => _children;
    public IList<UiDisplay> Displays => _displays;
    #endregion
    
    private readonly ObservableValue<Rect> _globalRect = new Rect((0, 0), (500, 500));
    private UiElement? _parent;
    private readonly ObservableList<UiElement> _children;
    private readonly ObservableList<UiDisplay> _displays = [];

    protected UiElement(UiElement[] children, Rect localAnchor, Margin margin, float globalZ)
    {
        LocalAnchor = localAnchor;
        Margin = margin;
        GlobalZ = globalZ;

        #region Children
        _children = new ObservableList<UiElement>(children.Length);
        _children.AfterElementAdded += child =>
        {
            child._parent = this;
            OnMeshesChanged();
        };
        _children.AfterElementRemoved += child =>
        {
            child._parent = null;
            OnMeshesChanged();
        };
        children.ForEach(ch => _children.Add(ch));
        #endregion
        
        #region Displayes

        _displays.AfterElementAdded += display =>
        {
            display.Parent = this;
            OnMeshesChanged();
        };
        _displays.AfterElementRemoved += display => 
        {
            display.Parent = null;
            OnMeshesChanged();
        };
        
        #endregion
        
        GlobalZ.OnChange += (oldValue, newValue) =>
        {
            var diff = newValue - oldValue;
            foreach (var display in Displays)
            {
                foreach (var mesh in display.Meshes)
                {
                    mesh.Mesh.Offset((0, 0, diff));
                }
            }
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
        LocalVisible.OnChange += (_, _) => OnMeshesChanged();


        _globalRect.OnChange += (_, newRect) => _children.ForEach(ch => ch.UpdateGlobalRect(newRect));
        _globalRect.OnChange += (oldR, newR) => Displays.ForEach(d => d.UpdateMeshes(oldR, newR));
    }
    
    private void NotifyParentAboutMeshChanges()
    {
        if (_parent is null)
        {
            Logger.Warn(this, $"can not notify cause {nameof(_parent)} is null");
            return;
        }
        _parent.OnMeshesChanged();
    }

    public virtual void OnMeshesChanged() => NotifyParentAboutMeshChanges(); // pass it up

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

    #region Debug

    // debug purpose
    protected void AddDebugArrowsToMeshes()
    {

    }

    #endregion
}