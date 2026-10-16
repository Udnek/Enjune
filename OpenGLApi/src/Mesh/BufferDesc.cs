using Enjune.Misc;
using OpenGLApi.Component;

namespace OpenGLApi.Mesh;

public readonly record struct BufferDesc
{
    public required int Binding { get; init; }
    public required string InShaderName { get; init; }
    public string? StructName
    {
        get => field ?? InShaderName+"_struct";
        init;
    }

    public required FieldDesc[] StructFields { get; init; }

    private static readonly Dictionary<Type, BufferDesc> Descriptions = [];
    
    public static ResultOrError<BufferDesc> Get<T>(int binding, Std std,
        string inShaderName, string? structName) where T : unmanaged
    {
        if (Descriptions.TryGetValue(typeof(T), out var desc))
            return ResultOrError.Success(desc);
        
        var structDescOrErr = StructDesc.Get<T>(std);
        if (structDescOrErr.Error is not null) 
            return structDescOrErr.Error.Value;
        var structDesc = structDescOrErr.GetOrThrow();

        var bufferDesc = new BufferDesc
        {
            Binding = binding,
            InShaderName = inShaderName,
            StructFields = structDesc.Fields,
            StructName = structName
        };
        Descriptions[typeof(T)] = bufferDesc;
        return ResultOrError.Success(bufferDesc);
    }
}