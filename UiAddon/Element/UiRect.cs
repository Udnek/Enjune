using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace UiAddon.Element;

public class UiRect : UiElement
{
    public readonly NotifyChange<Color> Color;

    public UiRect(UiElement[] children, Rect localAnchor, Margin margin, float globalZ, Color color) : base(children, localAnchor, margin, globalZ)
    {
        Color = color;
        Color.OnChange += (_, _) => OnColorChange();
    }

    private void OnColorChange()
    {
        for (var i = 0; i < Meshes.Count; i++) 
            Meshes[i] = Meshes[i].WithColor(Color);
    }
    
    protected override void UpdateShape(Rect oldValue, Rect newValue)
    {
        Meshes.Clear();
        var min = new Vector3(GlobalRect.Min.X, GlobalRect.Min.Y, GlobalZ);
        var max = new Vector3(GlobalRect.Max.X, GlobalRect.Max.Y, GlobalZ);
        Meshes.Add(new Model.Entry(
            Mesh.Quad(
                min, (max.X, min.Y, GlobalZ), max, (min.X, max.Y, GlobalZ),
                TextureQuad.Full),
            new Model.PerMesh(Color))
        );
    }
}