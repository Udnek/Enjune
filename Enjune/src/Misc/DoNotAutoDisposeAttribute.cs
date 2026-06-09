namespace Enjune.Misc;


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DoNotAutoDisposeAttribute(string reason) : Attribute
{
    public readonly string Reason = reason;
}