using System.Reflection;
using Enjune.Misc;

namespace Enjune.Data;


public record Codec<TInstance>(
    Func<TInstance, DataObject> Encode,
    Func<DataObject, TInstance> Decode)
{
    
    public Codec<TInstance?> Nullable
    {
        get
        {
            field ??= new Codec<TInstance?>(
                instance => instance is null ? DataObject.Null : Encode(instance),
                data => data == DataObject.Null ? default : Decode(data));
            return field;
        }
    }
    
    public Codec<TInstance[]> Array
    {
        get
        {
            field ??= new Codec<TInstance[]>(
                instances => new DataObject.Array(instances.Select(i => Encode(i)).ToArray()), 
                data =>
                {
                    var array = data.AsOr(DataObject.Array.Empty);
                    return array.Val.Select(v => Decode(v)).ToArray();
                });

            return field;
        }
    }
}

public delegate T Getter<in TInstance, out T>(TInstance instance);
public delegate void Setter<TInstance, in T>(ref TInstance instance, T val);
public delegate void DecodeAndSet<TInstance>(ref TInstance instance, DataObject? data); 

public static partial class Codecs
{
    public static readonly Codec<float> Float = new(
        v => new DataObject.Number((decimal) v),
        data => (float) data.AsOr(DataObject.Number.Zero).Decimal
    );
    
    public static readonly Codec<bool> Boolean = new(
        DataObject.Boolean.Of,
        data => data.AsOr(DataObject.Boolean.False).Val
    );
    
    public static readonly Codec<string> String = new(
        v => new DataObject.String(v),
        data => data.AsOr(DataObject.String.Empty).Val
    );

    public static readonly Codec<Assembly> Assembly = new(
        v => new DataObject.String(v.FullName ?? throw new InvalidOperationException()),
        data => System.Reflection.Assembly.Load(data.AsOr(DataObject.String.Empty).Val)
    );
    
    public static readonly Codec<Vector3> Vector3 = ForEmptyConstructor(() => new Vector3())
            .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
            .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float)
            .ForField("z", v => v.Z, (ref v, val) => v.Z = val, Float)
            .Build();
    
    public static readonly Codec<Quaternion> Quaternion = ForEmptyConstructor(() => new Quaternion())
        .ForField("x", i => i.X, (ref i, val) => i.X = val, Float)
        .ForField("y", i => i.Y, (ref i, val) => i.Y = val, Float)
        .ForField("z", i => i.Z, (ref i, val) => i.Z = val, Float)
        .ForField("w", i => i.W, (ref i, val) => i.W = val, Float, 1)
        .Build();


    public static BigConstructorBuilder<TInstance> ForConstructor<TInstance>(Func<object?[], TInstance> constructor) 
        => new(constructor);

    public static EmptyConstructorBuilder<TInstance> ForEmptyConstructor<TInstance>(Func<TInstance> newInstanceCreator) 
        => new(newInstanceCreator);
}