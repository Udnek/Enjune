using Enjune.Graphic.GraphicApi.OpenGL.Component.Array;
using Enjune.Misc;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component;

public sealed class VaoAttributes<T> where T : unmanaged
{
    private readonly List<Attribute> _attributes = [];
    private readonly Vao _vao;
    private readonly AbstractBuffer<T> _vbo;
    private readonly ShaderProgram _shaderProgram;
    private bool _compiled = false;
    
    public VaoAttributes(Vao vao, Vbo<T> vbo, ShaderProgram program)
    {
        _vao = vao;
        _vbo = vbo;
        _shaderProgram = program;
    }
    
    public void Add<TT>(VertexAttribPointerType pType, string name, int elements, bool perInstance = false) where TT : unmanaged
    {
        if (_compiled)
        {
            Logger.Error(this, "trying to compiled already compiled");
            return;
        }
        unsafe
        {
            int size =  sizeof(TT) * elements;
            _attributes.Add(new Attribute(size, name, elements, pType, perInstance));   
        }
    }

    public void Compile()
    {
        if (_compiled)
        {
            Logger.Error(this, "trying to compiled already compiled");
            return;
        }
        _compiled = true;
        
        _vao.Bind();
        _vbo.Bind();
        _shaderProgram.Bind();
        
        int stride = _attributes.Sum(a => a.SizeBytes);
        int offset = 0;
        foreach (var attr in _attributes)
        {
            int location = _shaderProgram.GetAttributeLocation(attr.Name);
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
