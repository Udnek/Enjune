using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Data;

namespace OpenGLApi.Mesh;

public interface IGlMeshDevice;

public readonly record struct VaoAttribute(
    string InShaderName,
    ShaderValuePrimitive Type,
    int Offset,
    int TotalByteSize,
    VertexAttribPointerType PointerType,
    int Location,
    bool IsInt);

public class VaoVboDevice<TPerVert> : IGlMeshDevice where TPerVert : unmanaged
{
    public required IReadOnlyList<VaoAttribute> VaoAttributes { get; init; }
    public required int Stride { get; init; }

    public void Create(bool isVboFixedSize, int vboSize, out Vbo<TPerVert> vbo, out Vao vao)
    {
        vao = new Vao();
        vbo = new Vbo<TPerVert>(vboSize, isVboFixedSize);
        
        // linking
        vao.Bind();
        vbo.Bind();
        
        foreach (var attr in VaoAttributes)
        {
            GL.EnableVertexAttribArray(attr.Location);
            if (attr.IsInt)
            {
                GL.VertexAttribIPointer(
                    attr.Location, attr.TotalByteSize,
                    (VertexAttribIntegerType) attr.PointerType,
                    Stride, attr.Offset);
            }
            else
            {
                GL.VertexAttribPointer(
                    attr.Location, attr.TotalByteSize,
                    attr.PointerType, false,
                    Stride, attr.Offset);
            }
        }   
    }
}

public static class VaoVboDevice
{
    private static readonly Dictionary<Type, IGlMeshDevice> Devices = [];

    public static ResultOrError<VaoVboDevice<TPerVert>> Get<TPerVert>() where TPerVert : unmanaged
    {
        if (Devices.TryGetValue(typeof(TPerVert), out var device))
            return ResultOrError.Success((VaoVboDevice<TPerVert>)device);

        Logger.Info(typeof(VaoVboDevice),
            $"Staring generation of device for {Logger.GetTypeName<TPerVert>()}");

        var type = typeof(TPerVert);
        if (type.IsPrimitive)
            return new Error($"can not use primitive {Logger.GetTypeName(type)}");

        var layout = type.GetCustomAttribute<StructLayoutAttribute>();
        if (layout is null)
            return new Error($"struct must has {Logger.GetTypeName(type)}");
        
        if (layout.Pack != 1)
            return new Error($"pack must be 1, got {layout.Pack}");

        #region Field Processing

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var vaoAttributes = new List<VaoAttribute>(fields.Length);
        var names = new HashSet<string>(fields.Length);
        int offset = 0;
        int loc = 0;
        foreach (var field in fields)
        {
            var fieldAttr = field.GetCustomAttribute<GraphicFieldAttribute>();
            if (fieldAttr is null)
                return new Error($"all fields must have {Logger.GetTypeName<GraphicFieldAttribute>()}, but {field.Name} don't");
            
            if (!names.Add(fieldAttr.InShaderName))
                return new Error($"names must be unique, got {fieldAttr.InShaderName} twice");

            var expectedSize = fieldAttr.Type.ByteSize();
            var realSize = Marshal.SizeOf(field.FieldType);
            if (expectedSize != realSize)
                return new Error($"Mismatch of sizes: specified {expectedSize}, but field is {realSize}");
            
            vaoAttributes.Add(new VaoAttribute
            {
                InShaderName = fieldAttr.InShaderName,
                Type = fieldAttr.Type,
                Offset = offset,
                TotalByteSize = realSize,
                PointerType = fieldAttr.Type == ShaderValuePrimitive.Int ? VertexAttribPointerType.Int : VertexAttribPointerType.Float,
                Location = loc,
                IsInt = fieldAttr.Type == ShaderValuePrimitive.Int
            });
            
            loc += 1;
            offset += realSize;
        }

        #endregion

        var vaoVboDevice = new VaoVboDevice<TPerVert>
        {
            VaoAttributes = vaoAttributes,
            Stride = offset // actually represents full struct size
        };
        Devices[type] = vaoVboDevice;
        return ResultOrError.Success(vaoVboDevice);
    }
}