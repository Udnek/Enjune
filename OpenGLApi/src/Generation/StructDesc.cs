using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using Enjune.Misc;

namespace OpenGLApi.Generation;

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
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        List<FieldDesc> descs = new List<FieldDesc>(fields.Length);
        HashSet<string> names = new HashSet<string>(fields.Length);
        foreach (var field in fields)
        {
            var fieldAttr = field.GetCustomAttribute<GraphicFieldAttribute>();
            if (fieldAttr is null)
                return new Error($"all fields must have {Logger.GetTypeName<GraphicFieldAttribute>()}, but {field.Name} don't");
            
            if (!names.Add(fieldAttr.InShaderName))
                return new Error($"names must be unique, got {fieldAttr.InShaderName} twice"); ;
            
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
