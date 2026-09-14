using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Key;
using Enjune.KitStart;
using Enjune.Misc;
using UiAddon.Display;
using UiAddon.Element;

namespace UiAddon.Presets;

public class EditableTextElement : UiElement
{
    public readonly TextDisplay TextDisplay;
    
    public EditableTextElement(
        UiElement[] children,
        TextDisplay textDisplay,
        Rect localAnchor,
        Margin margin,
        float globalZ
        ) : base(children, [textDisplay], localAnchor, margin, globalZ)
    {
        TextDisplay = textDisplay;
        IsHovered.OnChange += (_, hovered)
            => TextDisplay.Color.Val += new Vector4(0.4f, 0.4f, 0.4f, 0) * (hovered ? 1 : -1);
    }
    
    public override BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
    {
        if (inputHandler.IsJustPressed(KeyCode.Escape)) 
            return BeingFocusedAction.StopBeing;
        var enterPressed = inputHandler.IsJustPressed(KeyCode.Enter);
        var backspacePressed = inputHandler.IsJustPressed(KeyCode.Backspace);
        if (!enterPressed && !backspacePressed && inputHandler.InputChars.Count == 0) 
            return BeingFocusedAction.ContinueBeing;
        
        if (enterPressed) 
            inputHandler.InputChars.Add('\n');

        var text = TextDisplay.Text.Val;
        text += new string(inputHandler.InputChars.AsSpan());
        if (backspacePressed && text.Length != 0) 
            text = text[..^1];

        TextDisplay.Text.Val = text;
        
        return BeingFocusedAction.ContinueBeing;
    }
}