namespace Enjune.Graphic.UI;

public class UiButton(Rect anchor, Margin margin, float z, params UiElement[] children)
    : UiElement(anchor, margin, z, children)
{
    private readonly Color _color = new(RandomFloat(), RandomFloat(), RandomFloat(), 1);

    private static float RandomFloat() => (float)new Random().NextDouble();

    protected override void GenerateMeshes()
    {
        base.GenerateMeshes();
        var min = new Vector3(GlobalRect.Min.X, GlobalRect.Min.Y, GlobalZ);
        var max = new Vector3(GlobalRect.Max.X, GlobalRect.Max.Y, GlobalZ);
        Meshes.Add(new Model.Entry(
            Mesh.Quad(
                min, (max.X, min.Y, GlobalZ), max, (min.X, max.Y, GlobalZ),
                TextureQuad.Full),
            new Model.PerMesh(_color)
            )
        );
    }
}