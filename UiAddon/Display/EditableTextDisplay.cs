using UiAddon.Display.Abstraction;
using UiAddon.Element;

namespace UiAddon.Display;

public class EditableTextDisplay : TextDisplay<EditableTextElement>
{
    public override void Initialize()
    {
        SubscribeToParent(Parent.Font, (_, _) => RegenerateMeshes());
        SubscribeToParent(Parent.TextHeight, (_, _) => RegenerateMeshes());
        SubscribeToParent(Parent.TextLines, (_, _) => RegenerateMeshes(), (_, _) => RegenerateMeshes());
        SubscribeToParent(Parent.ObservableCursorPosition, (_, _) => RegenerateMeshes());
        base.Initialize();
    }

    protected override void OnRectChange(Rect oldRect, Rect rect)
    {
        RegenerateMeshes();
    }

    protected override void RegenerateMeshes()
    {
        Meshes.Clear();
        CreateMeshes(Parent.TextLines, Parent.Font, Parent.TextHeight, Alignment.Top, m =>
        {
            if ((m.line, m.CharIndex) == Parent.CursorPos)
            {
                m.Mesh = m.Mesh.WithColor(new Color(1, 0, 0, 1));
            }
            Meshes.Add(m.Mesh);
        });
    }
}