using System.Reflection;
using System.Text;
using Enjune.Misc;
using Enjune.Registering;

namespace Enjune.Data;

//public delegate TInstance Decoder<TInstance>(DataObject data, TInstance fallback);

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
        instance => new DataObject.Number((decimal) instance),
        data => (float) data.AsOr(DataObject.Number.Zero).Decimal);
    
    public static readonly Codec<int> Int = new(
        instance => new DataObject.Number(instance),
        data => (int) data.AsOr(DataObject.Number.Zero).Decimal);
    
    public static readonly Codec<bool> Boolean = new(
        DataObject.Boolean.Of,
        data => data.AsOr(DataObject.Boolean.False).Val);
    
    public static readonly Codec<string> String = new(
        instance => new DataObject.String(instance),
        data => data.AsOr(DataObject.String.Empty).Val);

    public static readonly Codec<Assembly> Assembly = new(
        instance => new DataObject.String(instance.FullName ?? throw new InvalidOperationException()),
        data => System.Reflection.Assembly.Load(data.AsOr(DataObject.String.Empty).Val));
    
    public static readonly Codec<Vector2> Vector2 = ForEmptyConstructor(() => new Vector2())
        .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
        .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float).Build();
    
    public static readonly Codec<Vector3> Vector3 = ForEmptyConstructor(() => new Vector3())
            .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
            .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float)
            .ForField("z", v => v.Z, (ref v, val) => v.Z = val, Float).Build();
    
    public static readonly Codec<Vector4> Vector4 = ForEmptyConstructor(() => new Vector4())
        .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
        .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float)
        .ForField("z", v => v.Z, (ref v, val) => v.Z = val, Float)
        .ForField("w", v => v.W, (ref v, val) => v.W = val, Float).Build();

    public static readonly Codec<Quaternion> Quaternion = new(
        q => Vector4.Encode(new Vector4(q.Xyz, q.Z)),
        data =>
        {
            var vec4 = Vector4.Decode(data);
            return new Quaternion(vec4.Xyz, vec4.W);
        });

    public static readonly Codec<Matrix4> Matrix4 = ForEmptyConstructor(() => new Matrix4())
        .ForField("row0", v => v.Row0, (ref v, val) => v.Row0 = val, Vector4)
        .ForField("row1", v => v.Row1, (ref v, val) => v.Row1 = val, Vector4)
        .ForField("row2", v => v.Row2, (ref v, val) => v.Row2 = val, Vector4)
        .ForField("row3", v => v.Row3, (ref v, val) => v.Row3 = val, Vector4)
        .Build();

    public static Codec<TInstance> ForRegistryEntry<TInstance>(
        Registry<TInstance> registry, Getter<TInstance, Identifier> idGetter, TInstance fallback)
    {
        return ForSingleConstructor(
            id => registry.GetOrDefault(id) ?? fallback, 
            idGetter,
            Identifier.Codec);
    }
    
    public static Codec<TInstance?> ForRegistryEntry<TInstance>(
        Registry<TInstance> registry, Getter<TInstance, Identifier> idGetter) where TInstance : class
    {
        return ForSingleConstructor(
            registry.GetOrDefault, 
            i => i is null ? Identifier.NullFallback : idGetter(i),
            Identifier.Codec);
    }

    public static Codec<TInstance> ForSingleConstructor<TInstance, TField>(Func<TField, TInstance> constructor, Getter<TInstance, TField> getter, Codec<TField> codec)
    {
        return new Codec<TInstance>(
            instance => codec.Encode(getter(instance)),
            data => constructor(codec.Decode(data)));
    }

    public static BigConstructorBuilder<TInstance> ForConstructor<TInstance>(Func<object?[], TInstance> constructor) 
        => new(constructor);

    public static EmptyConstructorBuilder<TInstance> ForEmptyConstructor<TInstance>(Func<TInstance> newInstanceCreator) 
        => new(newInstanceCreator);
    
    public static EitherBuilder<TInstance> ForEither<TInstance>() => new();
}