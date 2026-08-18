namespace Enjune.Misc;

public readonly record struct Error(string Text)
{
    public static implicit operator Error(string text) => new(text);

    public static implicit operator string(Error error) => error.Text;

    public override string ToString() => Text;
    
    public void Log(object source)
    {
        if (Text.Length == 0)
            Logger.Error(source, Text);
        else
            Logger.Error(source, string.Concat(Text.First().ToString().ToUpper(), Text.AsSpan(1)));
    }
}