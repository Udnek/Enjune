using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using Enjune.Misc;

namespace OpenGLApi.Mesh;

public class StructDesc<T> where T : unmanaged
{
    
}

public static class SsboDesc
{
    private static readonly Dictionary<Type, object> Devices = [];

    public static ResultOrError<StructDesc<T>> Get<T>() where T : unmanaged
    {
        if (Devices.TryGetValue(typeof(T), out var device))
            return ResultOrError.Success((StructDesc<T>)device);

        Logger.Info(typeof(SsboDesc),
            $"Staring generation of desc for {Logger.GetTypeName<T>()}");

        var type = typeof(T);
        if (type.IsPrimitive)
            return new Error($"can not use primitive {Logger.GetTypeName(type)}");

        var layout = type.GetCustomAttribute<StructLayoutAttribute>();
        if (layout is null)
            return new Error($"struct must has {Logger.GetTypeName(type)}");
        
        if (layout.Pack != 1)
            return new Error($"pack must be 1, got {layout.Pack}");

        #region Field Processing

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        List<VaoAttribute> vaoAttributes = new List<VaoAttribute>(fields.Length);
        HashSet<string> names = [];
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
                PointerType = fieldAttr.Type == ValuePrimitive.Int ? VertexAttribPointerType.Int : VertexAttribPointerType.Float,
                Location = loc,
                IsInt = fieldAttr.Type == ValuePrimitive.Int
            });
            
            loc += 1;
            offset += realSize;
        }

        #endregion

        var vaoVboDevice = new VaoVboDevice<T>
        {
            VaoAttributes = vaoAttributes,
            Stride = offset // actually represents full struct size
        };
        Devices[type] = vaoVboDevice;
        return ResultOrError.Success(vaoVboDevice);
    }
}