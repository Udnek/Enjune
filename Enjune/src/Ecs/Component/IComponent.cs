using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Misc;
using Enjune.Registering;

namespace Enjune.Ecs.Component;

public interface IComponent
{
    Identifier Id();
    
    #region Codec

    private const string IdKey = "id";
    private const string ComponentKey = "component";

    private static readonly ICodec<(Identifier Id, IComponent Comp)> KeyedCodec = new SimpleCodec<(Identifier Id, IComponent Comp)>(
        tuple =>
        {
            var (id, comp) = tuple;
            var idResult = Identifier.Codec.Encode(id);
            if (idResult.Error != null)
                return new Error($"can not encode {id}: {idResult.Error}");
            
            var compCodec = Registries.Codec.Get(id, out var err);
            if (compCodec is null)
                return new Error($"can not encode {tuple}: {err}");

            var compResult = compCodec.EncodeObj(tuple);
            if (compResult.Error != null)
                return new Error($"can not encode {tuple}: {compResult.Error}");

            return ResultOrError.Success<DataObject>(new Dictionary<string, DataObject>()
            {
                [IdKey] = idResult.GetOrThrow(),
                [ComponentKey] = compResult.GetOrThrow()
            });
        },
        data =>
        {
            var map = data.Cast<DataObject.Map>(out var castErr);
            if (map is null) 
                return new Error($"can not decode: {castErr}");
            
            // id
            if (!map.Val.TryGetValue(IdKey, out var idData))
                return new Error($"can not find 'id' in {map}");
            
            var idResult = Identifier.Codec.Decode(idData);
            if (idResult.Error != null)
                return new Error($"can not decode: {idResult.Error}");
            var compCodec = Registries.Codec.Get(idResult.GetOrThrow(), out var regErr);
            if (compCodec is null)
                return new Error($"can not decode: {regErr}");

            // data
            if (!map.Val.TryGetValue(ComponentKey, out var compData))
                return new Error($"can not find 'data' in {compData}");

            var compResult = compCodec.DecodeObj(compData);
            if (compResult.Error != null)
                return new Error($"can not decode: {compResult.Error}");

            if (compResult.GetOrThrow() is not IComponent comp)
                return new Error($"decoded comp is of incorrect type: {compResult.GetOrThrow()}");

            return ResultOrError.Success((idResult.GetOrThrow(), comp));
        });
    
    public static readonly ICodec<IComponent> Codec = new SimpleCodec<IComponent>(
        comp => KeyedCodec.Encode((comp.Id(), comp)),
        data => KeyedCodec.Decode(data).AndThen(val => val.Comp));

    public static readonly ICodec<IComponent[]> ArrayCodec = Codecs.ArrayOf(Codec, true);

    #endregion
}