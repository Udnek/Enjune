using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Element;
using UiAddon.Layout;

namespace UiAddon.Display.Abstraction;

public abstract class TextDisplay<TParent> : ColoredDisplay<TParent> where TParent : IUiElement
{
    protected abstract void RegenerateMeshes();

    /// <summary>
    /// returns lines' y baseline
    /// </summary>
    /// <param name="lineIndex"></param>
    /// <param name="totalLines"></param>
    /// <param name="lineText"></param>
    /// <param name="font"></param>
    /// <param name="height"></param>
    /// <param name="alignment"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected float CreateMeshes(
        int lineIndex,
        int totalLines,
        string lineText,
        CompiledFont font,
        float height,
        Alignment alignment,
        Action<(Model.Entry Mesh, int CharIndex)> action)
    {
        var perMeshData = new Model.PerMesh(font.Material, Color.Val);
        var rect = Parent.Rect.Val;
        float yOffset;
        switch (alignment)
        {
            case Alignment.Top:
                yOffset = rect.Max.Y - height;
                break;
            case Alignment.Center:
                var center = rect.Min.Y + rect.Size.Y/2;
                var textSize = totalLines * height;
                yOffset = center - textSize/2;
                break;
            case Alignment.Bottom:
                yOffset = rect.Min.Y;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        font.GenerateMeshes(lineText, height, mesh =>
        {
            mesh.Mesh.Offset((rect.Min.X, yOffset + lineIndex * -height, Z));
            action((new Model.Entry(mesh.Mesh, perMeshData), mesh.CharIdx));
        });
        return yOffset + lineIndex * -height;
    }

    protected void CreateMeshes(
        IList<string> textLines,
        CompiledFont font,
        float height,
        Alignment alignment,
        Action<(Model.Entry Mesh, int line, int CharIndex)> action)
    {
        for (var i = textLines.Count - 1; i >= 0; i--)
        {
            CreateMeshes(i, textLines.Count, textLines[i], font, height, alignment, 
                m =>
                {
                    action((m.Mesh, i, m.CharIndex));
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