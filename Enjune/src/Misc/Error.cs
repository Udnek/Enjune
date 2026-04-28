namespace Enjune.Misc;

public readonly record struct Error(string Text)
{
    public static implicit operator Error(string text) => new(text);

    public static implicit operator string(Error error) => error.Text;

    public override string ToString() => Text;
    
    public void Log(object source) => Logger.Error(source, this);
}