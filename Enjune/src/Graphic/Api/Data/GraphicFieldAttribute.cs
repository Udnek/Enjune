namespace Enjune.Graphic.Api.Data;

[AttributeUsage(AttributeTargets.Field)]
public class GraphicFieldAttribute : System.Attribute
{
    public readonly ShaderValuePrimitive Type;
    public readonly string InShaderName;

    public GraphicFieldAttribute(ShaderValuePrimitive type, string inShaderName)
    {
        Type = type;
        InShaderName = inShaderName;
        if (!IsValidName(inShaderName))
            throw new Exception($"Vertex attribute name must contain only letters, digits and underscores, got {inShaderName}");
    }

    private static bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name)) 
            return false;
        return name.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');
    }
}