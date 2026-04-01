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
    
    public void Add<T>(string name, int elements) where  T : unmanaged
    {
        unsafe
        {
            int size = sizeof(T) * elements;
            _attributes.Add(new Attribute(size, name, elements));
        }
    }

    public void Compile()
    {
        int stride = _attributes.Sum(a => a.SizeBytes);
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
            offset += attr.SizeBytes;
        }
    }
}

public record struct Attribute(int SizeBytes, string Name, int Elements);