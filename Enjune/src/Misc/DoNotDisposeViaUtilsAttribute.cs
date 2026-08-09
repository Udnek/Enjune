namespace Enjune.Misc;


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DoNotDisposeViaUtilsAttribute(string reason) : Attribute
{
    public readonly string Reason = reason;
}