using Enjune.Graphic;
using Enjune.Graphic.Modeling;

namespace UiAddon.Display;

public class DebugArrowsBoundsDisplay(float zOffset) : UiDisplay(zOffset)
{
    public override void UpdateMeshes(Rect _, Rect rect)
    {
        var anchorSize = MathF.Max(10, MathF.Sqrt(rect.Size.X + rect.Size.Y));
        var color = Color.One;
        {
            var minAnchor = Mesh.Triangle((0.5f, 0, 0), (1, 1, 0), (0, 0.5f, 0), TextureQuad.Full);
            minAnchor.Offset((-1, -1, 0));
            minAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            minAnchor.Offset(new Vector3(rect.Min));
            minAnchor.Offset((0, 0, Z));
            Meshes.Add(new Model.Entry(minAnchor, new Model.PerMesh(color)));
        }
        {
            var maxAnchor = Mesh.Triangle((0f, 0, 0), (1f, 0.5f, 0), (0.5f, 1, 0), TextureQuad.Full);
            maxAnchor.Multiply(new Vector3(anchorSize)); // just resizing to be visible on screen;
            maxAnchor.Offset(new Vector3(rect.Max));
            maxAnchor.Offset((0, 0, Z));
            Meshes.Add(new Model.Entry(maxAnchor, new Model.PerMesh(color)));
        }
    }
}