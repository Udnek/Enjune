namespace Enjune.Graphic.Input.UI;

public sealed class UiDirectory(Rect anchor, Margin margin, float z, params UiElement[] children) : UiElement(anchor, margin, z, children);