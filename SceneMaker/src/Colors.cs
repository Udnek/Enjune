using Enjune.Misc;

namespace SceneMaker;

public static class Colors
{
    public static readonly Color Red = System.Drawing.Color.Red.ToTk();
    public static readonly Color Green = System.Drawing.Color.Green.ToTk();
    public static readonly Color Blue = System.Drawing.Color.Blue.ToTk();
    
    public static readonly Color UiBackground = System.Drawing.Color.FromArgb(255, 20, 68, 47).ToTk();
    public static readonly Color UiText = System.Drawing.Color.FromArgb(255, 189, 208, 215).ToTk();
}