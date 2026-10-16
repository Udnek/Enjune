using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using Enjune.Misc;
using OpenGLApi.Component;

namespace OpenGLApi.Mesh;

public readonly record struct StructDesc
{
    public required FieldDesc[] Fields { get; init; }


    private static readonly Dictionary<Type, StructDesc> Descriptions = [];

    public static ResultOrError<StructDesc> Get<T>(Std? std) where T : unmanaged
    {
        var stdErr = std?.Check<T>();
        if (stdErr is not null) return stdErr.Value;
        
        if (Descriptions.TryGetValue(typeof(T), out var desc))
            return ResultOrError.Success(desc);

        Logger.Info(typeof(StructDesc),
            $"Staring description generation for {Logger.GetTypeName<T>()}");
        
        #region Field Processing

        var fields = typeof(T)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            // ordering by offset, cause GetFields order is not guaranteed;
            .OrderBy(fi => fi.GetCustomAttribute<FieldOffsetAttribute>()!.Value).ToArray();

        List<FieldDesc> descs = new List<FieldDesc>(fields.Length);
        HashSet<string> names = new HashSet<string>(fields.Length);
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
            
            descs.Add(new FieldDesc
            {
                InShaderName = fieldAttr.InShaderName,
                Type = fieldAttr.Type
            });
        }

        #endregion

        var structDesc = new StructDesc
        {
            Fields = descs.ToArray()
        };
        Descriptions[typeof(T)] = structDesc;
        Logger.Info(typeof(StructDesc), "Done");
        
        return ResultOrError.Success(structDesc);
    }
}
