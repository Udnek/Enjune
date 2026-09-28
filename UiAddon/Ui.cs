using System.Diagnostics.Contracts;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;
using UiAddon.Element;
using UiAddon.Layout;

namespace UiAddon;

public sealed class Ui : AbstractDisposable
{
    static Ui()
    {
        Logger.RegisterNamespaceToDomain(typeof(Ui).Assembly, "", new Logger.Domain("UiAddon", ConsoleColor.DarkCyan));
    }

    #region Public

    public readonly ObservableValue<float> PixelsPerUnit = 1;
    public bool IsFocused => FocusedElement is not null;
    public IUiElement? FocusedElement { get; private set; }
    public readonly List<IUiElement> Roots;
    
    #endregion

    private Rect _rect;
    public ILayout _layout = new FlexBoxLayout()
    {
        Padding = Margin.Inside(30),
        MainMode = FlexBoxLayout.DimensionBehaviour.Grow,
        CrossMode = FlexBoxLayout.DimensionBehaviour.Grow,
        ContentDirection = FlexBoxLayout.Direction.LeftToRight,
        ChildGap = 20
    };
    private Matrix4 _projectionTransform;
    private readonly BasicInputHandler _inputHandler;
    private readonly DynamicRenderableModel _model;

    public Ui(IGraphicApi graphicApi, BasicInputHandler inputHandler, IUiElement[] roots)
    {
        Roots = new List<IUiElement>(roots);
        _inputHandler = inputHandler;
        _model = new DynamicRenderableModel(graphicApi);
        RecalculateRect();
        RecollectMeshes();

        PixelsPerUnit.ObserveAsOwner((_, _) => RecalculateRect());
    }

    /// <summary>
    /// Should be called before Render each frame
    /// </summary>
    public void Update(float deltaTime)
    {
        // updating size
        if (_inputHandler.WindowSizeChanged) 
            RecalculateRect();
        
        // updating hovered
        RecheckHoveredElements();

        if (Roots.Any(r => r.ParentShouldUpdateMyRect))
            _layout = _layout.UpdateSelfLayoutAndChildrenRects(_rect, Roots);
        
        // calling recursive update
        Roots.ForEach(r => r.RecursiveUpdate(deltaTime));
        
        // checking if meshes changed
        bool recollectMeshes = false;
        RecursiveChildrenExplore(child =>
        {
            if (child.MeshesChanged)
            {
                recollectMeshes = true;
                child.MeshesChanged = false;
            }
            return true;
        });
        
        if (recollectMeshes)
        {
            RecollectMeshes();
            _model.Refit();
        }
    }

    /// <summary>
    /// Should be called after Update each frame
    /// </summary>
    /// <param name="shader"></param>
    public void Render(IShader.ICamera.IColor shader)
    {
        shader.ModelTransform(Matrix4.Identity);
        shader.ViewTransform(Matrix4.Identity);
        shader.ProjectionTransform(_projectionTransform);
        _model.Render(shader);
    }
    
    private void RecollectMeshes()
    {
        _model.Clear();
        RecursiveChildrenExplore(elem =>
        {
            if (!elem.LocalVisible) return false;
            elem.Displays.ForEach(d => d.Meshes.ForEach(m => _model.Add(m)));
            return true;
        });
    }
    
    private void RecalculateRect()
    {
        var x = _inputHandler.WindowSize.X / PixelsPerUnit;
        var y = _inputHandler.WindowSize.Y / PixelsPerUnit;
        _rect = new Rect((0f, 0f), (x, y));
        _projectionTransform = Matrix4.CreateOrthographicOffCenter(0, x, 0, y, -100, 100);
        _layout = _layout.UpdateSelfLayoutAndChildrenRects(_rect, Roots);
    }
    
    private void RecheckHoveredElements()
    {
        var cursor = _inputHandler.CursorPosition;
        var correctedCursor = new Vector2(cursor.X / PixelsPerUnit, cursor.Y / PixelsPerUnit);
        IUiElement? newFocus = null;
        var anythingHovered = false;
        RecursiveChildrenExplore(elem =>
        {
            if (!elem.LocalVisible) return false;
            var isHovered = elem.Rect.Val.ContainsPoint(correctedCursor);
            elem.IsHovered.Val = isHovered;
            if (!isHovered) return true;
            anythingHovered = true;
            
            var action = elem.UpdateBeingHovered(_inputHandler);
            if (action == IUiElement.BeingHoveredAction.BecomeFocused) 
                newFocus = elem;
            return true;
        });
        
        if (!anythingHovered && _inputHandler.IsJustPressed(KeyCode.LeftMouseButton)) // we are clicked out of Ui
        {
            FocusedElement?.IsFocused.Val = false;
            FocusedElement = null;
            return;
        }
        
        if (newFocus is not null)
        {
            FocusedElement?.IsFocused.Val = false;
            FocusedElement = newFocus;
            newFocus.IsFocused.Val = true;
        }
        
        if (FocusedElement is null) 
            return;
        
        var action = FocusedElement.UpdateBeingFocused(_inputHandler);
        if (action == IUiElement.BeingFocusedAction.StopBeing)
        {
            FocusedElement.IsFocused.Val = false;
            FocusedElement = null;
        }
    }
    
    // Misc

    private void RecursiveChildrenExplore(Func<IUiElement, bool> takeAndContinue)
    {
        Roots.ForEach(Explore);
        return;
        
        void Explore(IUiElement element)
        {
            var @continue = takeAndContinue(element);
            if (!@continue) return;
            element.Children.ForEach(Explore);
        }
    }

    protected override void DisposeData() => _model.Dispose();
}