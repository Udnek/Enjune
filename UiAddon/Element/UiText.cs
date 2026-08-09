using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace UiAddon.Element;

public class UiText : UiElement
{
    public readonly NotifyChange<CompiledFont> Font;
    public readonly NotifyChange<string> Text;
    public readonly NotifyChange<Color> Color;
    
    public UiText(UiElement[] children, Rect localAnchor, Margin margin, float globalZ, CompiledFont font, string text, Color color) : base(children, localAnchor, margin, globalZ)
    {
        Font = font;
        Text = text;
        Color = color;

        Color.OnChange += OnColorChanged;
        Font.OnChange += (_, _) => RegenerateMeshesEntirely();
        Text.OnChange += (_, _) => RegenerateMeshesEntirely();
    }

    private void OnColorChanged(Color old, Color newColor)
    {
        for (var i = 0; i < Meshes.Count; i++)
        {
            Meshes[i] = Meshes[i].WithColor(newColor);
        }
        NotifyParentAboutMeshChanges();
    }

    protected void RegenerateMeshesEntirely()
    {
        Meshes.Clear();
        var perMeshData = new Model.PerMesh(Font.Val.Material, Color.Val);
        var split = Text.Val.Split('\n');
        for (var i = 0; i < split.Length; i++)
        {
            var line = split[i];
            Font.Val.GenerateMeshes(line, GlobalRect.Val.Height, mesh =>
            {
                mesh.Offset(new Position(GlobalRect.Val.Min.X, GlobalRect.Val.Min.Y - i*GlobalRect.Val.Height, GlobalZ + 0.1f));
                Meshes.Add(new Model.Entry(mesh, perMeshData));
            });
        }
        NotifyParentAboutMeshChanges();
    }

    protected override void UpdateShape(Rect oldValue, Rect newValue) => RegenerateMeshesEntirely();
}