namespace Enjune.Misc;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DisposeAtLastAttribute(string reason) : Attribute
{
    public readonly string Reason = reason;
}