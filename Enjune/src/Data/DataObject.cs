using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Enjune.Data.Json;
using Enjune.Misc;

namespace Enjune.Data;

public abstract class DataObject
{
    public static readonly NullVal Null = NullVal.Instance;

    [Pure]
    public T? Cast<T>(out Error? error) where T : DataObject
    {
        if (this is T thisT)
        {
            error = null;
            return thisT;
        }
        error = $"can not cast {this} to ${Logger.GetTypeName<T>()}";
        return null;
    }
    
    public override string ToString() => JsonSerde.Tight.Serialize(this);

    public static implicit operator DataObject(Dictionary<string, DataObject> val) => new Map(val);
    public static implicit operator DataObject(DataObject[] val) => new Array(val);
    public static implicit operator DataObject(string val) => new String(val);
    public static implicit operator DataObject(bool val) => Boolean.Of(val);
    public static implicit operator DataObject(decimal val) => new Number(val);

    // primitives
    
    public sealed class Array(DataObject[] val) : DataObject
    {
        public static readonly Array Empty = new([]);
        
        public Span<DataObject> Val => val;
    }

    public sealed class Map(IReadOnlyDictionary<string, DataObject> val) : DataObject
    {
        public static readonly Map Empty = new(ReadOnlyDictionary<string, DataObject>.Empty);
        
        public IReadOnlyDictionary<string, DataObject> Val { get; } = val;
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