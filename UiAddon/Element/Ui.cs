using System.Diagnostics.Contracts;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;

namespace UiAddon.Element;

public sealed class Ui : UiElement, IDisposable
{
    static Ui()
    {
        Logger.RegisterNamespaceToDomain(typeof(Ui).Assembly, "", new Logger.Domain("UiAddon", ConsoleColor.DarkCyan));
    }

    // public api
    public readonly NotifyChange<float> PixelsPerUnit = 1;
    public bool IsFocused => FocusedElement is not null;
    public UiElement? FocusedElement { get; private set; }
    
    // matrices
    private readonly Matrix4 _modelTransform = Matrix4.Identity;
    private readonly Matrix4 _viewTransform = Matrix4.Identity;
    private Matrix4 _projectionTransform;
    
    private readonly BasicInputHandler _inputHandler;
    private readonly IRenderableModel.IDynamic _model;
    private readonly List<Model.Entry> _meshes = [];
    private bool _someMeshesChanged = false;

    public Ui(IGraphicApi graphicApi, BasicInputHandler inputHandler, UiElement[] roots) : base(roots, Anchor.FullStretch, UiAddon.Margin.No, 0)
    {
        _inputHandler = inputHandler;
        UpdateAllRects();
        RecollectMeshes();
        _model = graphicApi.CreateDynamicRenderable(CreateModel());

        PixelsPerUnit.OnChange += (_, _) => UpdateAllRects();
    }

    public void Update()
    {
        if (_inputHandler.WindowSizeChanged) 
            UpdateAllRects();
        
        RecheckHoveredElements();
        
        if (_someMeshesChanged)
        {
            RecollectMeshes();
            _model.Refit(CreateModel());
        }
    }

    public void Render(IShader.ICamera.IColor shader)
    {
        shader.ModelTransform(_modelTransform);
        shader.ViewTransform(_viewTransform);
        shader.ProjectionTransform(_projectionTransform);
        _model.Render(shader);
    }

    private void RecollectMeshes()
    {
        _someMeshesChanged = false;
        _meshes.Clear();
        RecursiveChildrenExplore(elem =>
        {
            if (!elem.LocalVisible) return false;
            _meshes.AddRange(elem.Meshes);
            return true;
        });
    }
    
    private void UpdateAllRects()
    {
        var x = _inputHandler.WindowSize.X / PixelsPerUnit;
        var y = _inputHandler.WindowSize.Y / PixelsPerUnit;
        var rect = new Rect((0f, 0f), (x, y));
        _projectionTransform = Matrix4.CreateOrthographicOffCenter(0, x, 0, y, -100, 100);
        UpdateGlobalRect(rect);
    }

    protected override void UpdateShape(Rect oldValue, Rect newValue)
    {
        Children.ForEach(ch => Upd(ch, newValue));
        return;
        
        void Upd(UiElement elem, Rect parentRect)
        {
            elem.UpdateGlobalRect(parentRect);
            elem.Children.ForEach(ch => Upd(ch, elem.GlobalRect));
        }
    }
    
    private void RecheckHoveredElements()
    {
        var cursor = _inputHandler.CursorPosition;
        var correctedCursor = new Vector2(cursor.X / PixelsPerUnit, cursor.Y / PixelsPerUnit);
        UiElement? newFocus = null;
        var anythingHovered = false;
        RecursiveChildrenExplore(elem =>
        {
            if (!elem.LocalVisible) return false;
            var isHovered = elem.GlobalRect.ContainsPoint(correctedCursor);
            elem.IsHovered.Val = isHovered;
            if (!isHovered) return true;
            anythingHovered = true;
            
            var action = elem.UpdateBeingHovered(_inputHandler);
            if (action == BeingHoveredAction.BecomeFocused) 
                newFocus = elem;
            return true;
        });

        if (!anythingHovered && _inputHandler.IsJustPressed(KeyCode.LeftMouseButton))
        {
            FocusedElement = null;
            return;
        }
        
        if (newFocus is not null) 
            FocusedElement = newFocus;

        var action = FocusedElement?.UpdateBeingFocused(_inputHandler) ?? BeingFocusedAction.StopBeing;
        if (action == BeingFocusedAction.StopBeing)
            FocusedElement = null;
    }

    [Pure]
    private Model CreateModel() => new(_meshes.ToArray());

    protected override void OnChildMeshesChanges() => _someMeshesChanged = true;

    // utils
    
    private void RecursiveChildrenExplore(Func<UiElement, bool> takeAndContinue)
    {
        Children.ForEach(Explore);
        return;
        
        void Explore(UiElement element)
        {
            var @continue = takeAndContinue(element);
            if (!@continue) return;
            element.Children.ForEach(Explore);
        }
    }
    
    public void Dispose() => _model.Dispose();
}