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
    ObservableValue<ILayout> Layout { get; }
    IReadonlyObservableValue<Rect> Rect { get; } // should only be modified via parent
    void SetRectAsParent(Rect newRect);

    ObservableValue<float> GlobalZ { get; }
    ObservableValue<bool> LocalVisible { get; } // self and children visibility
    ObservableValue<bool> IsHovered { get; }
    ObservableValue<bool> IsFocused { get; }

    /// <summary>
    /// Element should set to true when becoming invisible or Displays should set to true when their meshes change
    /// </summary>
    bool MeshesChanged { get; set; }

    IList<IUiElement> Children { get; }
    IList<IUiDisplay> Displays { get; }
    bool ParentShouldUpdateMyRect { get; }

    /// <summary>
    /// Called from Ui's Update each frame
    /// </summary>
    /// <param name="deltaTime"></param>
    void RecursiveUpdate(float deltaTime);
    
    #region Hovering and Focusing

    /// <summary>
    /// Should decide if element become focused when being hovered
    /// </summary>
    /// <param name="inputHandler"></param>
    /// <returns></returns>
    BeingHoveredAction UpdateBeingHovered(BasicInputHandler inputHandler);

    /// <summary>
    /// Should decide if element continue being focused
    /// </summary>
    /// <param name="inputHandler"></param>
    /// <returns></returns>
    BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler);
    
    enum BeingHoveredAction
    {
        BecomeFocused,
        DoNotBecomeFocused
    }
    enum BeingFocusedAction
    {
        ContinueBeing,
        StopBeing
    }
    
    #endregion
}

[LogParams(logCallingMethod: true)]
public class AbstractUiElement : IUiElement
{
    #region Public

    public IReadonlyObservableValue<Rect> Rect => _rect;
    public ObservableValue<ILayout> Layout { get; }
    public ObservableValue<bool> LocalVisible { get; } = true;
    public ObservableValue<bool> IsHovered { get; } = false;
    public ObservableValue<bool> IsFocused { get; } = false;
    public ObservableValue<float> GlobalZ { get; }

    public bool MeshesChanged { get; set; } = false;
    IList<IUiElement> IUiElement.Children => _children;
    public IList<IUiElement> Children => _children;
    public IList<IUiDisplay> Displays => _displays;
    public bool ParentShouldUpdateMyRect { get; private set; }

    #endregion
    
    private ObservableValue<Rect> _rect { get; } = new Rect((0, 0), (500, 500)); // initially set to notice bugs earlier
    private readonly ObservableList<IUiElement> _children;
    private readonly ObservableList<IUiDisplay> _displays;

    public AbstractUiElement(ILayout layout, float globalZ = 0, IEnumerable<IUiElement>? children = null)
    {
        GlobalZ = globalZ;
        Layout = new ObservableValue<ILayout>(layout);

        children ??= Array.Empty<IUiElement>();
        _children = new ObservableList<IUiElement>(children.Count());
        children.ForEach(ch => _children.Add(ch));
        
        _displays = new ObservableList<IUiDisplay>();
        _displays.AfterItemAddedAsOwner((_, display) =>
        {
            if (display.Meshes.Count > 0)
                MeshesChanged = true;
            display.Initialize();
        });
        _displays.AfterItemRemovedAsOwner((_, display) => 
        {
            if (display.Meshes.Count > 0)
                MeshesChanged = true;
            display.UnsubscribeFromParent();
        });
        
        GlobalZ.ObserveAsOwner((oldValue, newValue) =>
        {
            var diff = newValue - oldValue;
            foreach (var display in Displays)
            {
                foreach (var mesh in display.Meshes)
                {
                    mesh.Mesh.Offset((0, 0, diff));
                }
            }
        });
        
        Layout.ObserveAsOwner((_, _) => ParentShouldUpdateMyRect = true);
        _rect.ObserveAsOwner((_, _) => OnSelfRectChange());
        LocalVisible.ObserveAsOwner((_, _) =>
        {
            if (Displays.Count > 0) 
                MeshesChanged = true;
        });
    }

    public void SetRectAsParent(Rect newRect) => _rect.Val = newRect;

    /// <summary>
    /// Should be used to update children
    /// </summary>
    private void OnSelfRectChange()
    {
        ParentShouldUpdateMyRect = false;
        Layout.Val = Layout.Val.UpdateSelfLayoutAndChildrenRects(Rect.Val, Children);
    }

    public void RecursiveUpdate(float deltaTime)
    {
        Children.ForEach(c => c.RecursiveUpdate(deltaTime));
    }
    
    public virtual IUiElement.BeingHoveredAction UpdateBeingHovered(BasicInputHandler inputHandler)
        => inputHandler.IsJustPressed(KeyCode.LeftMouseButton)
            ? IUiElement.BeingHoveredAction.BecomeFocused
            : IUiElement.BeingHoveredAction.DoNotBecomeFocused;

    public virtual IUiElement.BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler) 
        => IsHovered ? IUiElement.BeingFocusedAction.ContinueBeing : IUiElement.BeingFocusedAction.StopBeing;
    
}