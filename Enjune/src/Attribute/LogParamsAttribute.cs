namespace Enjune.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class LogParamsAttribute(
    LogParamsAttribute.Method method = LogParamsAttribute.Method.FancyTypeToString,
    bool logCallingMethod = false
    ) : System.Attribute
{
    public readonly Method Mtd = method;
    public readonly bool LogCallingMethod = logCallingMethod;

    public enum Method
    {
        ToString,
        FancyTypeToString // Logger.GetTypeName
    }
}