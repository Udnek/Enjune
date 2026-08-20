using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;
using Enjune.Data.Codec.Misc;
using Enjune.Misc;
using Enjune.Registering;

namespace Enjune.Data.Codec;

public interface IObjCodec
{
    [Pure]
    ResultOrError<DataObject> EncodeObj(object? instance);
    [Pure]
    ResultOrError<object?> DecodeObj(DataObject data);
}

public interface ICodec<T> : IObjCodec
{
    [Obsolete("use generic method")]
    ResultOrError<object?> IObjCodec.DecodeObj(DataObject data)
    {
        var result = Decode(data);
        return ResultOrError.Convert<T, object?>(result);
    }

    [Obsolete("use generic method")]
    ResultOrError<DataObject> IObjCodec.EncodeObj(object? instance)
    {
        if (instance is T)
            return EncodeObj(instance);
        return new Error($"Can not encode {instance} because expected {Logger.GetTypeName<T>()}; use generic method");
    }

    [Pure]
    ResultOrError<DataObject> Encode(T instance);
    [Pure]
    ResultOrError<T> Decode(DataObject data);
}

public static class Codecs
{
    #region Primitives
    
    public static readonly SimpleCodec<float> Float = new(
        instance => ResultOrError.Success<DataObject>(new DataObject.Number((decimal) instance)),
        data => 
        {
            var number = data.Cast<DataObject.Number>(out var castErr);
            if (number is null)
                return new Error("can not decode float: " + castErr);
            return ResultOrError.Success((float)number.Decimal);
        });
    
    public static readonly SimpleCodec<int> Int = new(
        instance => ResultOrError.Success<DataObject>(instance),
        data => 
        {
            var number = data.Cast<DataObject.Number>(out var castErr);
            if (number is null)
                return new Error("can not decode int: " + castErr);
            var isInt = number.Decimal % 1 == 0;
            if (!isInt)
                return new Error($"{number.Decimal} is not whole number");
            return ResultOrError.Success((int)number.Decimal);
        });
    
    public static readonly SimpleCodec<bool> Boolean = new(
        instance => ResultOrError.Success<DataObject>(instance),
        data =>
        {
            var boolean = data.Cast<DataObject.Boolean>(out var castErr);
            if (boolean is null)
                return new Error("can not decode bool: " + castErr);
            return ResultOrError.Success(boolean.Val);
        });
    
    public static readonly SimpleCodec<string> String = new(
        instance => ResultOrError.Success<DataObject>(instance),
        data =>
        {
            var str = data.Cast<DataObject.String>(out var castErr);
            if (str is null)
                return new Error("can not decode str: " + castErr);
            return ResultOrError.Success(str.Val);
        });

    #endregion
    
    #region Math
    
    public static readonly MapCodec<Vector2> Vector2 = new MapCodec<Vector2>.Builder(() => new Vector2())
        .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
        .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float).Build();

    public static readonly MapCodec<Vector3> Vector3 = new MapCodec<Vector3>.Builder(() => new Vector3())
        .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
        .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float)
        .ForField("z", v => v.Z, (ref v, val) => v.Z = val, Float).Build();
    
    public static readonly MapCodec<Vector4> Vector4 = new MapCodec<Vector4>.Builder(() => new Vector4())
        .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
        .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float)
        .ForField("z", v => v.Z, (ref v, val) => v.Z = val, Float)
        .ForField("w", v => v.W, (ref v, val) => v.W = val, Float).Build();

    public static readonly MapCodec<Quaternion> Quaternion = new MapCodec<Quaternion>.Builder(() => new Quaternion())
        .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
        .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float)
        .ForField("z", v => v.Z, (ref v, val) => v.Z = val, Float)
        .ForField("w", v => v.W, (ref v, val) => v.W = val, Float).Build();

    public static readonly MapCodec<Matrix4> Matrix4 = new MapCodec<Matrix4>.Builder(() => new Matrix4())
        .ForField("row0", v => v.Row0, (ref v, val) => v.Row0 = val, Vector4)
        .ForField("row1", v => v.Row1, (ref v, val) => v.Row1 = val, Vector4)
        .ForField("row2", v => v.Row2, (ref v, val) => v.Row2 = val, Vector4)
        .ForField("row3", v => v.Row3, (ref v, val) => v.Row3 = val, Vector4).Build();
    
    #endregion

    #region Assebly
    public static readonly SimpleCodec<Assembly> Assembly = new(
        instance =>
        {
            var name = instance.FullName;
            if (name is not null) return ResultOrError.Success<DataObject>(name);
            return new Error($"can not encode assembly {instance} cause name is null");
        },
        data =>
        {
            var str = data.Cast<DataObject.String>(out var castErr);
            if (str is null)
                return new Error("can not decode assembly name: " + castErr);
            Assembly assembly;
            try
            {
                assembly = System.Reflection.Assembly.Load(str.Val);
            }
            catch(Exception e)
            {
                return new Error("can not load assembly: " + e);
            }
            return ResultOrError.Success(assembly);
        });
    #endregion

    #region Utils
    
    public static SimpleCodec<T?> NullableOf<T>(ICodec<T> codec) where T : class
    {
        return new SimpleCodec<T?>(
            instance => instance is null ? ResultOrError.Success<DataObject>(DataObject.Null) : codec.Encode(instance),
            data =>
            {
                if (data == DataObject.Null)
                    return ResultOrError.Success<T?>(null);
                else
                    return ResultOrError.Convert<T, T?>(codec.Decode(data));
            });
    }
    public static SimpleCodec<T?> NullableOfStruct<T>(ICodec<T> codec) where T : struct
    {
        return new SimpleCodec<T?>(
            instance => instance is null ? ResultOrError.Success<DataObject>(DataObject.Null) : codec.Encode((T)instance),
            data =>
            {
                if (data == DataObject.Null)
                    return ResultOrError.Success<T?>(null);
                else
                    return codec.Decode(data).Map(
                        val => ResultOrError.Success<T?>(val),
                        err => err);
            });
    }
    
    public static SimpleCodec<T[]> ArrayOf<T>(ICodec<T> codec, bool skipInvalidItems = false)
    {
        return new SimpleCodec<T[]>(
            instances =>
            {
                List<DataObject> encoded = new (instances.Length);
                foreach (var instance in instances)
                {
                    var result = codec.Encode(instance);
                    if (result.Error == null)
                    {
                        encoded.Add(result.GetOrThrow());
                        continue;
                    }

                    if (skipInvalidItems)
                        Logger.Warn(typeof(Codecs), $"Can not decode {instance}: {result.Error}");
                    else
                        return new Error($"can not encode {instance}: {result.Error}");
                }

                return ResultOrError.Success<DataObject>(new DataObject.Array(encoded.ToArray()));
            }, 
            data =>
            {
                var array = data.Cast<DataObject.Array>(out var error);
                if (array is null)
                    return new Error($"can not decode {array}: " + error);

                var decoded = new List<T>(array.Val.Length);
                foreach (var item in array.Val)
                {
                    var err = codec.Decode(item).Map<Error?>(
                        value =>
                        {
                            decoded.Add(value);
                            return null;
                        },
                        err => err);

                    if (err == null) continue;
                    if (skipInvalidItems)
                        Logger.Warn(typeof(Codecs), $"Could not decode item {item} in array {array}");
                    else
                        return new Error(err);

                }

                return ResultOrError.Success(decoded.ToArray());
            });
    }

    #endregion

    #region Builders

    public static InstanceMatchCodec<TInstance>.Builder ForMatchInstance<TInstance>() => new();

    public static MapCodec<TInstance>.Builder ForEmptyConstructor<TInstance>(EmptyConstructor<TInstance> newInstanceCreator) 
        => new(newInstanceCreator);
    
    public static SingleArgConstructorCodec<TInstance, TField> ForOneArgConstructor<TInstance, TField>(
        Func<TField, TInstance> constructor, Getter<TInstance, TField> getter, ICodec<TField> codec)
    {
        return new SingleArgConstructorCodec<TInstance, TField>(constructor, getter, codec);
    }
    public static ConstructorCodec<TInstance>.Builder ForConstructor<TInstance>(Func<List<object?>, TInstance> constructor) 
        => new(constructor);

    #endregion
    


}