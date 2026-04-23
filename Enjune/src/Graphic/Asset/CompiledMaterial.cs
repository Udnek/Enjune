namespace Enjune.Graphic.Asset;

public readonly record struct CompiledMaterial(RawMaterial Raw, MatId Id, TexId TextureId)
{
    public override string ToString() 
        => $"{nameof(CompiledMaterial)} {{Id: {Id}; TexId: {TextureId}; raw: {Raw}}}";
}