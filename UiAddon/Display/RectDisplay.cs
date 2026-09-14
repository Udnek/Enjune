using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Display.Abstraction;
using UiAddon.Element;

namespace UiAddon.Display;

public class RectDisplay() : ColoredDisplay<IUiElement>
{
    protected override void OnRectChange(Rect _, Rect rect)
    {
        Logger.Highlight(this, "Updating rect");
        Meshes.Clear();
        var min = new Vector3(rect.Min.X, rect.Min.Y, Z);
        var max = new Vector3(rect.Max.X, rect.Max.Y, Z);
        Meshes.Add(new Model.Entry(
            Mesh.Quad(
                min, (max.X, min.Y, Z), 
                max, (min.X, max.Y, Z),
                TextureQuad.Full),
            new Model.PerMesh(Color))
        );
    }
}