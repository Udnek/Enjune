using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Element;
using UiAddon.Layout;

namespace UiAddon.Display.Abstraction;

public abstract class TextDisplay<TParent> : ColoredDisplay<TParent> where TParent : TextElement
{
    public override void Initialize()
    {
        SubscribeToParent(Parent.TextLines, (_, _) => RegenerateMeshes(), (_, _) => RegenerateMeshes());
        base.Initialize();
    }

    protected abstract void RegenerateMeshes();

    protected void CreateMeshes(IList<string> textLines, CompiledFont font, float height, Alignment alignment, Action<(Model.Entry Mesh, int line, int CharIndex)> action)
    {
        var perMeshData = new Model.PerMesh(font.Material, Color.Val);
        var rect = Parent.Rect.Val;
        float initialYOffset;
        switch (alignment)
        {
            case Alignment.Top:
                initialYOffset = rect.Max.Y - height;
                break;
            case Alignment.Center:
                var center = rect.Min.Y + rect.Size.Y/2;
                var textSize = textLines.Count * height;
                initialYOffset = center - textSize/2;
                break;
            case Alignment.Bottom:
                initialYOffset = rect.Min.Y;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        for (var lineIdx = textLines.Count - 1; lineIdx >= 0; lineIdx--)
        {
            var line = textLines[lineIdx];
            font.GenerateMeshes(line, height, mesh =>
            {
                mesh.Mesh.Offset((rect.Min.X, initialYOffset + lineIdx * -height, Z));
                action((new Model.Entry(mesh.Mesh, perMeshData), lineIdx, mesh.CharIdx));
            });
        }
    }
    
    public enum Alignment
    {
        Top,
        Center,
        Bottom
    }
}