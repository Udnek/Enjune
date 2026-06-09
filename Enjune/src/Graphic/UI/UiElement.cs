using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace Enjune.Graphic.UI;

public abstract class UiElement(UiElement[]? children, List<Model.Entry> meshes)
{
    public float GlobalZ { get; set; }
    public Rect GlobalRect { get; protected set; }
    public abstract bool IsHovered { get; set; }
    public bool LocalHidden { get; set; }
    public readonly UiElement[]? Children = children;
    public readonly List<Model.Entry> Meshes = meshes;

    public abstract void UpdateOnlySelfRect(Rect parent);
    public void UpdateSelfAndChildrenRect(Rect parent)
    {
        UpdateOnlySelfRect(parent);
        Children?.ForEach(c => c.UpdateSelfAndChildrenRect(GlobalRect));
    }

    public abstract void RegenerateSelfMeshes();
    public void RegenerateSelfAndVisibleChildrenMeshes()
    {
        RegenerateSelfMeshes();
        Children?.ForEach(c =>
        {
            if (c.LocalHidden) return;
            c.RegenerateSelfAndVisibleChildrenMeshes();
        });
    }
}