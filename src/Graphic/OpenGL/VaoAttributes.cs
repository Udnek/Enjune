using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL;

public class VaoAttributes
{
    private readonly List<Attribute> _attributes = [];
    private readonly ShaderProgram _shaderProgram;

    public VaoAttributes(ShaderProgram program)
    {
        _shaderProgram = program;
    }
    
    public void Add(Attribute attribute) => _attributes.Add(attribute);

    public void Compile()
    {
        int stride = _attributes.Sum(a => a.Elements) * sizeof(float);
        int offset = 0;

        foreach (var attr in _attributes)
        {
            int location = _shaderProgram.GetAttributeLocation(attr.Name);
            GL.EnableVertexAttribArray(location);
            GL.VertexAttribPointer(
                location,
                attr.Elements,
                VertexAttribPointerType.Float,
                false,
                stride,
                offset);
            offset += attr.Elements * sizeof(float);
        }
    }
}

public struct Attribute(string name, int elements)
{
    public readonly string Name = name;
    public readonly int Elements = elements;
}