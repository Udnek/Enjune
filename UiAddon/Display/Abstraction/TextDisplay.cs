using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace UiAddon.Display;

public abstract class TextDisplay : ColoredDisplay
{
    public readonly ObservableValue<CompiledFont> Font;
    public readonly ObservableValue<string> Text;
    public readonly ObservableValue<float> Size;

    protected TextDisplay(float zOffset, Color color, CompiledFont font, string text, float size) : base(zOffset, color)
    {
        Font = font;
        Text = text;
        Size = size;

        Font.OnChange += (_, _) => UpdateMeshes();
        Text.OnChange += (_, _) => UpdateMeshes();
        Size.OnChange += (_, _) => UpdateMeshes();
    }

    protected abstract void UpdateMeshes();
    
    protected void CreateMeshes(Action<Model.Entry> action)
    {
        var perMeshData = new Model.PerMesh(Font.Val.Material, Color.Val);
        var split = Text.Val.Split('\n');
        for (var i = 0; i < split.Length; i++)
        {
            var line = split[i];
            Font.Val.GenerateMeshes(line, Size, mesh =>
            {
                action(new Model.Entry(mesh, perMeshData));
            });
        }
    }
}