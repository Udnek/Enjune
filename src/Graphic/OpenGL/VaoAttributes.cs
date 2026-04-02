using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Graphic.OpenGL.Component;
using Enjune.Graphic.OpenGL.Component.Array;
using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL;

public sealed class VaoAttributes<T> where T : unmanaged
{
    private readonly List<Attribute> _attributes = [];
    private readonly Vao _vao;
    private readonly VaoOwnedBuffer<T> _vbo;
    private readonly ShaderProgram _shaderProgram;
    private bool _compiled = false;

    // public static VaoAttributes<T> Empty(Vao vao, VaoOwnedBuffer<T> buffer, 
    //     VertexAttribPointerType pointerType, ShaderProgram program)
    // {
    //     return new VaoAttributes<T>(vao, buffer, pointerType, program);
    // }

    // public static VaoAttributes<T> FromStruct(Vao vao, VaoOwnedBuffer<T> buffer, ShaderProgram program)
    // {
    //     if (typeof(T).IsPrimitive) 
    //         throw new ArgumentException("value must be struct");
    //
    //     var attributes = new VaoAttributes<T>(vao, buffer, VertexAttribPointerType.Float, program);
    //     
    //     foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
    //     {
    //         var size = Marshal.SizeOf(field.FieldType);
    //         var name =  field.Name;
    //         int elements = field.FieldType switch
    //         {
    //             Type t when t.IsPrimitive => 1,
    //             Type t when t == typeof(Vector3) => 3,
    //             Type t when t == typeof(Vector2) => 2,
    //             _ => throw Exception("can not recongnize ")
    //         };
    //         attributes.Add(new Attribute());
    //         // Logger.Log(typeof(VaoAttributes<T>), 
    //         //     $"field: {field.Name}, {field.FieldType}, " +
    //         //     $"{Marshal.SizeOf(field.FieldType)} {Marshal.OffsetOf(typeof(T), field.Name)}");
    //     }
    //
    //     return null!;
    // }
    
    public VaoAttributes(Vao vao, Vbo<T> vbo, ShaderProgram program)
    {
        _vao = vao;
        _vbo = vbo;
        _shaderProgram = program;
    }
    
    public void Add<TT>(VertexAttribPointerType pType, string name, int elements) where TT : unmanaged
    {
        if (_compiled)
        {
            Logger.Error(this, "trying to compiled already compiled");
            return;
        }
        unsafe
        {
            int size =  sizeof(TT) * elements;
            _attributes.Add(new Attribute(size, name, elements, pType));   
        }
    }

    public void Compile()
    {
        if (_compiled)
        {
            Logger.Error(this, "trying to compiled already compiled");
            return;
        }
        _vao.Bind();
        _vbo.Bind();
        _shaderProgram.Bind();
        
        int stride = _attributes.Sum(a => a.SizeBytes);
        int offset = 0;
        foreach (var attr in _attributes)
        {
            int location = _shaderProgram.GetAttributeLocation(attr.Name);
            GL.EnableVertexAttribArray(location);
            GL.VertexAttribPointer(
                location,
                attr.Elements,
                attr.PointerType,
                false,
                stride,
                offset);
            offset += attr.SizeBytes;
        }
        
        _shaderProgram.Unbind();
        _vao.Unbind();
        _vbo.Unbind();
        
    }
    
    private record struct Attribute(int SizeBytes, string Name, int Elements, VertexAttribPointerType PointerType);
    
    // private static int PointerTypeByteSize(VertexAttribPointerType type)
    // {
    //     return type switch
    //     {
    //         VertexAttribPointerType.Byte or
    //             VertexAttribPointerType.UnsignedByte => 1,
    //     
    //         VertexAttribPointerType.Short or
    //             VertexAttribPointerType.UnsignedShort or
    //             VertexAttribPointerType.HalfFloat => 2,
    //     
    //         VertexAttribPointerType.Int or
    //             VertexAttribPointerType.UnsignedInt or
    //             VertexAttribPointerType.Float or
    //             VertexAttribPointerType.Fixed or
    //             VertexAttribPointerType.Int2101010Rev => 4,
    //     
    //         VertexAttribPointerType.Double => 8,
    //     
    //         _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    //     };
    // }
}
