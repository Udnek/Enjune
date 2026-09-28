using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using UiAddon.Display.Abstraction;
using UiAddon.Element;

namespace UiAddon.Display;

public class DebugArrowsBoundsDisplay : AbstractUiDisplay<IUiElement>
{
    protected override void OnRectChange(Rect _, Rect rect)
    {
        var anchorSize = MathF.Max(10, MathF.Sqrt(rect.Size.X + rect.Size.Y));
        var color = Color.One;
        {
            var minAnchor = Mesh.Triangle(
                (0.5f, 0, 0), (1, 1, 0), (0, 0.5f, 0), 
                TextureQuad.Full, PrimitiveTopology.Triangle, false);
            minAnchor.Move((-1, -1, 0));
            minAnchor.Scale(new Vector3(anchorSize)); // just resizing to be visible on screen;
            minAnchor.Move(new Vector3(rect.Min));
            minAnchor.Move((0, 0, Z));
            Meshes.Add(new MeshInstance.Entry
            {
                Geometry = minAnchor,
                Color = color
            });
        }
        {
            var maxAnchor = Mesh.Triangle((0f, 0, 0), (1f, 0.5f, 0), (0.5f, 1, 0), 
                TextureQuad.Full, PrimitiveTopology.Triangle, false);
            maxAnchor.Scale(new Vector3(anchorSize)); // just resizing to be visible on screen;
            maxAnchor.Move(new Vector3(rect.Max));
            maxAnchor.Move((0, 0, Z));
            Meshes.Add(new MeshInstance.Entry
            {
                Geometry = maxAnchor,
                Color = color
            });
        }
    }
}