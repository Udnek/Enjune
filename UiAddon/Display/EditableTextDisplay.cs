using System.Text;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Display.Abstraction;
using UiAddon.Element;

namespace UiAddon.Display;

public class EditableTextDisplay : TextDisplay<EditableTextElement>
{

    public ObservableValue<Color> CursorColor { get; init; } = new Color(1, 1, 1, 0.6f);
    public ObservableValue<Color> LineBackgroundColor { get; init; } = new Color(1, 1, 1, 0.1f);

    private int? CursorMeshIndex = null;
    private int? BackLineMeshIndex = null;

    public override void Initialize()
    {
        SubscribeToParent(Parent.Font, (_, _) => RegenerateMeshes());
        SubscribeToParent(Parent.TextHeight, (_, _) => RegenerateMeshes());
        SubscribeToParent(Parent.Text, (_, _) => RegenerateMeshes(), (_, _) => RegenerateMeshes());
        SubscribeToParent(Parent.ObservableCursorPosition, (_, _) => RegenerateMeshes());
        CursorColor.ObserveAsOwner((_, c) =>
        {
            if (CursorMeshIndex is null) return;
            Meshes[CursorMeshIndex.Value] = Meshes[CursorMeshIndex.Value].WithColor(c); 
        });
        LineBackgroundColor.ObserveAsOwner((_, c) =>
        {
            if (BackLineMeshIndex is null) return;
            Meshes[BackLineMeshIndex.Value] = Meshes[BackLineMeshIndex.Value].WithColor(c); 
        });
        base.Initialize();
    }

    protected override void OnRectChange(Rect oldRect, Rect rect)
    {
        RegenerateMeshes();
    }

    protected override void RegenerateMeshes()
    {
        Meshes.Clear();
        CursorMeshIndex = null;
        BackLineMeshIndex = null;

        int cursorCharIdx = Parent.CursorPos - Parent.Text.FirstBefore(Parent.CursorPos, '\n')-1;
        int cursorLineIndex = Parent.Text.AsSpan().Slice(0, Parent.CursorPos).Count('\n');
        var lines = new string(Parent.Text.AsSpan()).Split("\n");
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
    
            var yBaseLine = CreateMeshes(lineIndex, Parent.Text.Count, line, Parent.Font, Parent.TextHeight,
                Alignment.Top, m => Meshes.Add(m.Mesh));
               
            if (cursorLineIndex == lineIndex)
            {
                // backline
                Meshes.Add(new Model.Entry(
                    Mesh.Quad((Parent.Rect.Val.Min.X, yBaseLine, Z-0.01f), Parent.Rect.Val.Width, 
                    Parent.TextHeight, TextureQuad.Full),
                    new Model.PerMesh(LineBackgroundColor)
                ));
                BackLineMeshIndex = Meshes.Count - 1; 

                // cursor
                var (width, _, _) = Parent.Font.Val.EstimateLineSize(
                    line.SafeSubstringFromTo(0, cursorCharIdx), Parent.TextHeight);
                Meshes.Add(new Model.Entry(
                    Mesh.Quad((Parent.Rect.Val.Min.X + width, yBaseLine, Z), Parent.TextHeight/15, 
                    Parent.TextHeight, TextureQuad.Full),
                    new Model.PerMesh(CursorColor)
                ));
                CursorMeshIndex = Meshes.Count - 1; 
            }
        }
    }
}