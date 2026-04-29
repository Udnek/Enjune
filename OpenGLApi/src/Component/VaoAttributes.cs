using Enjune.Misc;
using OpenGLApi.Component.Buffer;

namespace OpenGLApi.Component;

public sealed class VaoAttributes
{
    private readonly List<Attribute> _attributes = [];
    private readonly Vao _vao;
    private readonly IVbo _vbo;
    private bool _compiled = false;
    
    public VaoAttributes(Vao vao, IVbo vbo)
    {
        _vao = vao;
        _vbo = vbo;
    }
    
    public VaoAttributes Add<TT>(VertexAttribPointerType pType, string name, int elements, bool perInstance = false) where TT : unmanaged
    {
        if (_compiled)
        {
            Logger.Error(this, "trying to modify, but already compiled");
            return this;
        }
        unsafe
        {
            int size =  sizeof(TT) * elements;
            _attributes.Add(new Attribute(size, name, elements, pType, perInstance));
            return this;
        }
    }

    public void Compile(ShaderProgram program)
    {
        if (_compiled)
        {
            Logger.Error(this, "trying to compile, but already compiled");
            return;
        }
        _compiled = true;
        
        _vao.Bind();
        _vbo.Bind();
        program.Bind();
        
        int stride = _attributes.Sum(a => a.SizeBytes);
        int offset = 0;
        foreach (var attr in _attributes)
        {
            int location = program.GetAttributeLocation(attr.Name);
            GL.EnableVertexAttribArray(location);
            var allIntPointerTypes = (int[]) Enum.GetValues(typeof(VertexAttribIntegerType));
            if (allIntPointerTypes.Contains((int) attr.PointerType)) // so it means we need to use integer pointer
            {
                GL.VertexAttribIPointer(
                    location,
                    attr.Elements,
                    (VertexAttribIntegerType) attr.PointerType,
                    stride,
                    offset);
            }
            else
            {
                GL.VertexAttribPointer(
                    location,
                    attr.Elements,
                    attr.PointerType,
                    false,
                    stride,
                    offset);
            }

            if (attr.PerInstance)
            {
                GL.VertexAttribDivisor(location, 1);
            }
            offset += attr.SizeBytes;
        }   
    }
    
    private record struct Attribute(int SizeBytes, string Name, 
        int Elements, VertexAttribPointerType PointerType, bool PerInstance);
}
