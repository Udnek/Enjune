using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.UI;

public sealed class Ui
{
    private readonly List<UiElement> _roots;
    private readonly List<Model.Entry> _meshes = [];
    
    public readonly Matrix4 ModelTransform = Matrix4.Identity;
    public readonly Matrix4 ViewTransform = Matrix4.Identity;
    public Matrix4 ProjectionTransform 
        => Matrix4.CreateOrthographicOffCenter(0, Size.X/PixelsPerUnit, 0, Size.Y/PixelsPerUnit, -10, 10);

    public Vector2 Size;
    public float PixelsPerUnit = 1;

    public Ui(Vector2 initialSize, params UiElement[] roots)
    {
        _roots = roots.ToList();
        Size = initialSize;
        UpdateAllRectsAndVisibleMeshes();
    }

    public void RecheckHoveredElements(Vector2i cursor)
    {
        var correctedCursor = new Vector2(cursor.X / PixelsPerUnit, cursor.Y / PixelsPerUnit);
        _roots.ForEach(Check);
        return;
        
        void Check(UiElement element)
        {
            if (element.LocalHidden) return;
            element.IsHovered = element.GlobalRect.IsPointIn(correctedCursor);
            element.Children?.ForEach(Check);
        }
    }

    public Model CreateModel()
    {
        _meshes.Clear();
        _roots.ForEach(GetMeshes);
        return new Model(_meshes.ToArray());

        void GetMeshes(UiElement element)
        {
            if (element.LocalHidden) return;
            _meshes.AddRange(element.Meshes);
            element.Children?.ForEach(GetMeshes);
        }
    }
    
    public void UpdateAllRectsAndVisibleMeshes()
    {
        var rect = new Rect((0, 0), Size/PixelsPerUnit);
        _roots.ForEach(child => child.UpdateSelfAndChildrenRect(rect));
        _roots.ForEach(child =>
        {
            if (child.LocalHidden) return;
            child.RegenerateSelfAndVisibleChildrenMeshes();
        });
    }
}