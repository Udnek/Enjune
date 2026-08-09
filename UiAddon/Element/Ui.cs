using System.Diagnostics.CodeAnalysis;
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
    
    // matrices
    private readonly Matrix4 _modelTransform = Matrix4.Identity;
    private readonly Matrix4 _viewTransform = Matrix4.Identity;
    private Matrix4 _projectionTransform;
    
    private readonly BasicInputHandler _inputHandler;
    private readonly IRenderableModel.IDynamic _model;
    private readonly List<Model.Entry> _meshes = [];
    private UiElement? FocusedElement { get; set; }
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
        ForeachChild(ch => Upd(ch, newValue));
        return;
        
        void Upd(UiElement elem, Rect parentRect)
        {
            elem.UpdateGlobalRect(parentRect);
            elem.ForeachChild(ch => Upd(ch, elem.GlobalRect.Val));
        }
    }
    
    private void RecheckHoveredElements()
    {
        var cursor = _inputHandler.CursorPosition;
        var correctedCursor = new Vector2(cursor.X / PixelsPerUnit, cursor.Y / PixelsPerUnit);
        UiElement? newFocus = null;
        RecursiveChildrenExplore(elem =>
        {
            if (!elem.LocalVisible) return false;
            var isHovered = elem.GlobalRect.Val.ContainsPoint(correctedCursor);
            elem.IsHovered.Val = isHovered;
            if (!isHovered) return true;
            
            var action = elem.UpdateBeingHovered(_inputHandler);
            if (action == BeingHoveredAction.BecomeFocused) 
                newFocus = elem;
            return true;
        });

        // if (newFocus is null) // we don't press any button in ui
        // {
        //     if (_inputHandler.IsJustPressed(KeyCode.LeftMouseButton)) // but we clicked somewhere else not in ui
        //     {
        //         FocusedElement = null; // we're unfocusing ui
        //         return;
        //     }
        // }
        
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
        ForeachChild(Explore);
        return;
        
        void Explore(UiElement element)
        {
            var @continue = takeAndContinue(element);
            if (!@continue) return;
            element.ForeachChild(Explore);
        }
    }
    
    public void Dispose() => _model.Dispose();
}