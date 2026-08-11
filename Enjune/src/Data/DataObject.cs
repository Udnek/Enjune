using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Enjune.Data.Json;
using Enjune.Misc;

namespace Enjune.Data;

public abstract class DataObject
{
    public static readonly NullVal Null = NullVal.Instance;
    
    [Pure]
    [Obsolete]
    private T AsOr<T>(T fallback, Error? errorToLog) where T : DataObject
    {
        if (this is T thisT) return thisT;
        if (errorToLog != null) Logger.Warn(this, errorToLog);
        return fallback;
    }

    [Pure]
    public T? Cast<T>(out Error? error) where T : DataObject
    {
        if (this is T thisT)
        {
            error = null;
            return thisT;
        }
        error = $"can not cast {this} to ${Logger.GetTypeName(typeof(T))}";
        return null;
    }
    
    [Pure]
    [Obsolete]
    public T AsOr<T>(T fallback) where T : DataObject 
        => AsOr(fallback, $"{this} is not a {Logger.GetTypeName(typeof(T))}, defaulting to {fallback}");

    public override string ToString() => JsonSerde.Tight.Serialize(this);

    public static implicit operator DataObject(Dictionary<string, DataObject> val) => new Map(val);

    // primitives
    
    public sealed class Array(DataObject[] val) : DataObject
    {
        public static readonly Array Empty = new([]);
        
        public Span<DataObject> Val => val;
        
        public static implicit operator Array(DataObject[] val) => new(val);
    }

    public sealed class Map(IReadOnlyDictionary<string, DataObject> val) : DataObject
    {
        public static readonly Map Empty = new(ReadOnlyDictionary<string, DataObject>.Empty);
        
        public IReadOnlyDictionary<string, DataObject> Val { get; } = val;
        
        // [Pure]
        // public T GetOr<T>(string key, T fallback) where T : DataObject
        // {
        //     if (Val.TryGetValue(key, out var value))
        //         return value as T ?? fallback;
        //
        //     return fallback;
        // }

        // [Pure]
        // public T? GetOrNull<T>(string key) where T : DataObject
        // {
        //     if (Val.TryGetValue(key, out var value))
        //         return value as T;
        //
        //     return null;
        // }
        
        public static implicit operator Map(Dictionary<string, DataObject> val) => new(val);
    }

    public sealed class Boolean : DataObject
    {
        public static readonly Boolean True = new(true);
        public static readonly Boolean False = new(false);

        [Pure]
        public static Boolean Of(bool val) => val ? True : False;
        
        public bool Val { get; }
        
        private Boolean(bool val) => Val = val;
        
        public static implicit operator Boolean(bool val) => val ? True : False;
    }

    public sealed class String(string val) : DataObject
    {
        public static readonly String Empty = new("");
        
        public static implicit operator String(string val) => new(val);
        public string Val { get; } = val;
    }
    
    public sealed class Number(decimal val) : DataObject
    {
        public static readonly Number Zero = new(0);
        
        public decimal Decimal { get; } = val;
    }
    
    public sealed class NullVal : DataObject
    {
        public static readonly NullVal Instance = new();
        private NullVal(){}
    }
}