using Enjune.Misc;

namespace UiAddon.Display;

public abstract class ColoredDisplay : UiDisplay
{
    public readonly ObservableValue<Color> Color;

    protected ColoredDisplay(float zOffset, Color color) : base(zOffset)
    {
        Color = color;
        Color.OnChange += (_, _) => UpdateColor();
    }
    
    private void UpdateColor()
    {
        for (var i = 0; i < Meshes.Count; i++) 
            Meshes[i] = Meshes[i].WithColor(Color);
    }
}