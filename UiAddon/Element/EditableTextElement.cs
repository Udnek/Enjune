using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Key;
using Enjune.KitStart;
using Enjune.Misc;
using UiAddon.Display;
using UiAddon.Display.Abstraction;
using UiAddon.Layout;

namespace UiAddon.Element;

public class EditableTextElement : TextElement
{
    public readonly ObservableValue<CompiledFont> Font;
    public readonly ObservableValue<float> TextHeight;
    public IReadonlyObservableValue<(int Line, int Idx)> ObservableCursorPosition => _observableCursorPosition;
    private readonly ObservableValue<(int Line, int Idx)> _observableCursorPosition = (0, 0);

    public (int Line, int Idx) CursorPos
    {
        get => _observableCursorPosition.Val;
        set
        {
            int line = Math.Clamp(value.Line, 0, TextLines.Count-1); // maxing cause Count can be 0
            int idx = Math.Clamp(value.Idx, 0, TextLines[line].Length);
            _observableCursorPosition.Val = (line, idx);
            Logger.Highlight(this, CursorPos);
        }
    }

    public EditableTextElement(
        IEnumerable<string> text,
        CompiledFont font,
        float textHeight,
        FlexBoxLayout layout,
        float globalZ = 0,
        IEnumerable<IUiElement>? children = null) : base(text, layout, globalZ, children)
    {
        Font = font;
        TextHeight = textHeight;
        
        TextLines.AfterItemAddedAsOwner((_, _) =>
        {
            CursorPos = CursorPos; // updating it to fit new lines
            UpdateDesiredSize();
        });
        TextLines.AfterItemRemovedAsOwner((_, _) =>
        {
            if (TextLines.Count == 0)
                TextLines.Add("");
            else
            {
                CursorPos = CursorPos; // updating it to fit new lines
                UpdateDesiredSize();
            }
        });
        Font.ObserveAsOwner((_, _) => UpdateDesiredSize());
        TextHeight.ObserveAsOwner((_, _) => UpdateDesiredSize());
    }

    protected void UpdateDesiredSize()
    {
        if (Layout.Val is not FlexBoxLayout flexBox)
        {
            Logger.Warn(this, $"only {nameof(FlexBoxLayout)} layout can be used with this element");
            return;
        }
        Logger.Highlight(this, TextLines.ContentToString());
        var textSize = Font.Val.EstimateTextSize(TextLines.AsSpan(), TextHeight, TextHeight);
        flexBox = flexBox with { DesiredSizeXy = (textSize.width, textSize.maxY - textSize.minY) };
        Layout.Val = flexBox;
    }

    public override IUiElement.BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
    {
        if (inputHandler.IsJustPressed(KeyCode.Escape)) 
            return IUiElement.BeingFocusedAction.StopBeing;

        // moving cursor
        {
            if (inputHandler.IsJustPressed(KeyCode.ArrowUp))
                CursorPos = CursorPos with { Line = CursorPos.Line - 1 };
            else if (inputHandler.IsJustPressed(KeyCode.ArrowDown))
                CursorPos = CursorPos with { Line = CursorPos.Line + 1 };
            if (inputHandler.IsJustPressed(KeyCode.ArrowLeft))
                CursorPos = CursorPos with { Idx = CursorPos.Idx - 1 };
            else if (inputHandler.IsJustPressed(KeyCode.ArrowRight))
                CursorPos = CursorPos with { Idx = CursorPos.Idx + 1 };
        }

        
        // adding input
        var enterPressed = inputHandler.IsJustPressed(KeyCode.Enter);
        var backspacePressed = inputHandler.IsJustPressed(KeyCode.Backspace);
        
        if (enterPressed)
        {
            var line = TextLines[CursorPos.Line];
            var first = line.SafeSubstringFromTo(0, CursorPos.Idx);
            var second = line.SafeSubstringFromTo(CursorPos.Idx + 1, line.Length);
            TextLines[CursorPos.Line] = first;
            TextLines.Insert(CursorPos.Line+1, second);
            CursorPos = CursorPos with { Line = CursorPos.Line + 1 };
        }
        else if (backspacePressed)
        {
            // removing \n
            if (CursorPos.Idx == 0)
            {
                // we have more than 1 line
                if (CursorPos.Line > 0 && TextLines.Count > 0)
                {
                    TextLines[CursorPos.Line - 1] += TextLines[CursorPos.Line];
                    TextLines.RemoveAt(CursorPos.Line);
                    CursorPos = (CursorPos.Line - 1, TextLines[CursorPos.Line-1].Length);
                }
            }
            else // just removing char
            {
                CursorPos = CursorPos with { Idx = CursorPos.Idx - 1 };
                TextLines[CursorPos.Line] = TextLines[CursorPos.Line].Remove(CursorPos.Idx, 1);
            }
        }
        else if (inputHandler.InputChars.Count > 0)
        {
            TextLines[CursorPos.Line] = TextLines[CursorPos.Line]
                .Insert(CursorPos.Idx, new string(inputHandler.InputChars.AsSpan()));
            CursorPos = CursorPos with { Idx = CursorPos.Idx + inputHandler.InputChars.Count };
        }
        
        return IUiElement.BeingFocusedAction.ContinueBeing;
    }
}