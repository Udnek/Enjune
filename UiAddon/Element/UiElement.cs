using System.Collections.ObjectModel;
using Enjune.Attribute;
using Enjune.Graphic;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;
using UiAddon.Display;
using UiAddon.Layout;

namespace UiAddon.Element;

public interface IUiElement
{
    // main properties
    public ILayoutData Layout { get; }
    public ObservableValue<Rect> Rect { get; } // should only be modified via parent
    
    public ObservableValue<float> GlobalZ { get; }
    public ObservableValue<bool> LocalVisible { get; } // self and children visibility
    public ObservableValue<bool> IsHovered { get; }
    public ObservableValue<bool> IsFocused { get; }
    
    IReadOnlyList<IUiElement> Children { get; }
    public IList<IUiDisplay> Displays { get; }
    bool ParentShouldUpdateMyRect { get; set; }
}

public interface IUiElement<TLayout> : IUiElement where TLayout : ILayoutData
{
    new ObservableValue<TLayout> Layout { get; }
}

[LogParams(logCallingMethod: true)]
public class AbstractUiElement<TSelfLayout, TChildLayout> : IUiElement<TSelfLayout> where TSelfLayout : ILayoutData where TChildLayout : ILayoutData
{
    #region Public

    ILayoutData IUiElement.Layout => Layout.Val;
    public ObservableValue<TSelfLayout> Layout { get; set; }
    public ObservableValue<Rect> Rect { get; } = new Rect((0, 0), (500, 500)); // initially set to notice bugs earlier
    
    public ObservableValue<float> GlobalZ { get; }
    public ObservableValue<bool> LocalVisible { get; } = false;
    public ObservableValue<bool> IsHovered { get; } = false;
    public ObservableValue<bool> IsFocused { get; } = false;
    
    public IReadOnlyList<IUiElement> Children => _children;
    public IList<IUiDisplay> Displays => _displays;
    public bool ParentShouldUpdateMyRect { get; set; }

    #endregion
    
    private readonly ObservableList<IUiElement<TChildLayout>> _children;
    private readonly ObservableList<IUiDisplay> _displays;

    public AbstractUiElement(TSelfLayout layout, float globalZ, IEnumerable<IUiElement<TChildLayout>> children)
    {
        Layout = layout;
        GlobalZ = globalZ;
        
        _children = new ObservableList<IUiElement<TChildLayout>>(children.Count());
        children.ForEach(ch => _children.Add(ch));
        
        _displays = new ObservableList<IUiDisplay>();
        _displays.AfterItemRemovedAsOwner(display => 
        {
            display.UnsubscribeFromParent();
        });
        
        GlobalZ.ObserveAsOwner((oldValue, newValue) =>
        {
            var diff = newValue - oldValue;
            foreach (var display in _displays)
            {
                foreach (var mesh in display.Meshes)
                {
                    mesh.Mesh.Offset((0, 0, diff));
                }
            }
        });
        Layout.ObserveAsOwner((_, _) => ParentShouldUpdateMyRect = true);
    }

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