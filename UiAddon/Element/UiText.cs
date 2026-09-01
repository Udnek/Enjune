using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace UiAddon.Element;

public class UiText : UiElement
{
    public readonly ObservableValue<CompiledFont> Font;
    public readonly ObservableValue<string> Text;
    public readonly ObservableValue<Color> Color;
    public readonly ObservableValue<float> TextSize;
    
    public UiText(UiElement[] children, Rect localAnchor, Margin margin, float globalZ, CompiledFont font, string text, float textSize, Color color) : base(children, localAnchor, margin, globalZ)
    {
        Font = font;
        Text = text;
        Color = color;
        TextSize = textSize;

        Color.OnChange += OnColorChanged;
        Font.OnChange += (_, _) => RegenerateMeshesEntirely();
        Text.OnChange += (_, _) => RegenerateMeshesEntirely();
        TextSize.OnChange += (_, _) => RegenerateMeshesEntirely();
    }

    private void OnColorChanged(Color old, Color newColor)
    {
        for (var i = 0; i < Displays.Count; i++) 
            Displays[i] = Displays[i].WithColor(newColor);
    }

    protected void RegenerateMeshesEntirely()
    {
        Displays.Clear();
        var perMeshData = new Model.PerMesh(Font.Val.Material, Color.Val);
        var split = Text.Val.Split('\n');
        for (var i = 0; i < split.Length; i++)
        {
            var line = split[i];
            Font.Val.GenerateMeshes(line, TextSize, mesh =>
            {
                mesh.Offset(new Position(GlobalRect.Min.X, GlobalRect.Min.Y - i*TextSize, GlobalZ));
                Displays.Add(new Model.Entry(mesh, perMeshData));
            });
        }
        AddDebugArrowsToMeshes();
    }

    protected override void UpdateShape(Rect oldValue, Rect newValue) => RegenerateMeshesEntirely();
}