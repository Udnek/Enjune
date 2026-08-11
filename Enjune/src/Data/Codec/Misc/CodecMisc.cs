using Enjune.Misc;

namespace Enjune.Data.Codec.Misc;

public delegate TInstance EmptyConstructor<out TInstance>();
public delegate TField Getter<in TInstance, out TField>(TInstance instance);
public delegate void Setter<TInstance, in TField>(ref TInstance instance, TField val);
public delegate DecodeResult<T> Decoder<T>(DataObject data);
public delegate DataObject Encoder<in T>(T instance);

public static class CodecMisc
{
    public static void ValidateFieldNames(IEnumerable<string> names)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var group in names.GroupBy(e => e))
        {
            if (group.Count() > 1) 
                // ReSharper disable once PossibleMultipleEnumeration
                Logger.Error(typeof(Codecs), $"field name used multiple times: \"{group.Key}\" in {names.ContentToString()}");
        }
    }
}