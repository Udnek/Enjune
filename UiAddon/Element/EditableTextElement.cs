using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Key;
using Enjune.KitStart;
using Enjune.Misc;
using UiAddon.Display;
using UiAddon.Display.Abstraction;
using UiAddon.Layout;

namespace UiAddon.Element;

public class EditableTextElement : AbstractUiElement
{
    public readonly ObservableList<char> Text;
    public readonly ObservableValue<CompiledFont> Font;
    public readonly ObservableValue<float> TextHeight;
    public IReadonlyObservableValue<int> ObservableCursorPosition => _observableCursorPosition;
    private readonly ObservableValue<int> _observableCursorPosition = 0;

    public int CursorPos
    {
        get => _observableCursorPosition.Val;
        set => _observableCursorPosition.Val = Math.Clamp(value, 0, Text.Count);
    }

    public EditableTextElement(
        IEnumerable<char> text,
        CompiledFont font,
        float textHeight,
        FlexBoxLayout layout,
        float globalZ = 0,
        IEnumerable<IUiElement>? children = null) : base(layout, globalZ, children)
    {
        Text = new ObservableList<char>(text);
        Font = font;
        TextHeight = textHeight;
        
        Text.AfterItemAddedAsOwner((_, _) =>
        {
            CursorPos = CursorPos; // updating it to fit new lines
            UpdateDesiredSize();
        });
        Text.AfterItemRemovedAsOwner((_, _) =>
        {
            CursorPos = CursorPos; // updating it to fit new lines
            UpdateDesiredSize();
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
        var textSize = Font.Val.EstimateTextSize(new string(Text.AsSpan()).Split('\n'), TextHeight, TextHeight);
        flexBox = flexBox with { DesiredSizeXy = (textSize.width, textSize.maxY - textSize.minY) };
        Layout.Val = flexBox;
    }

    public override IUiElement.BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
    {
        // unfocusing
        if (inputHandler.IsJustPressed(KeyCode.Escape)) 
            return IUiElement.BeingFocusedAction.StopBeing;

        // moving cursor
        if (inputHandler.IsJustPressed(KeyCode.ArrowUp))
        {
            var lineStart = Text.FirstBefore(CursorPos, '\n')+1;
            if (lineStart > 0)
            {
                var previousLine = Text.FirstBefore(lineStart-1, '\n')+1;
                if (Text[previousLine] == '\n')
                    CursorPos = previousLine;
                else
                    CursorPos = Math.Min(
                        previousLine + (CursorPos - lineStart),  // line begin + column
                        Text.FirstAfter(previousLine, '\n')); // or just line len
            }
        }
        else if (inputHandler.IsJustPressed(KeyCode.ArrowDown))
        {
            var lineEnd = Text.FirstAfter(CursorPos-1, '\n');
            if (lineEnd < Text.Count)
            {
                var nextLine = lineEnd + 1;
                if (Text[nextLine] == '\n')
                    CursorPos = nextLine;
                else
                {
                    var lineStart = Text.FirstBefore(CursorPos, '\n') + 1;
                    CursorPos = Math.Min(
                        nextLine + (CursorPos - lineStart), // line begin + column
                        Text.FirstAfter(nextLine, '\n')); // or just line len
                }
            }
        }
        if (inputHandler.IsJustPressed(KeyCode.ArrowLeft))
            CursorPos -= 1;
        else if (inputHandler.IsJustPressed(KeyCode.ArrowRight))
            CursorPos += 1;

        // adding input
        if (inputHandler.IsJustPressed(KeyCode.Enter))
        {
            Text.Insert(CursorPos, '\n');
            CursorPos += 1;
        }
        else if (inputHandler.IsJustPressed(KeyCode.Backspace))
        {
            if (Text.Count > 0 && CursorPos > 0)
            {
                CursorPos -= 1;
                Text.RemoveAt(CursorPos);
            }
        }
        else if (inputHandler.IsJustPressed(KeyCode.Tab))
        {
            Text.Insert(CursorPos, '\t');
            CursorPos += 1;
        }
        else if (inputHandler.InputChars.Count > 0)
        {
            foreach (var c in inputHandler.InputChars)
            {
                Text.Insert(CursorPos, c);
                CursorPos += 1;
            }
        }
        
        return IUiElement.BeingFocusedAction.ContinueBeing;
    }
}