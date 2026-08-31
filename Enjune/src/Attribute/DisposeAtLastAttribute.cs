namespace Enjune.Attribute;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DisposeAtLastAttribute(string reason) : System.Attribute
{
    public readonly string Reason = reason;
}