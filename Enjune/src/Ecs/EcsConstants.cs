namespace Enjune.Ecs;

public static class EcsConstants
{
    public const EntityId MaxEntities = 8192;
    public static readonly SignatureInteger SignatureWidth = (SignatureInteger) Math.Log2(SignatureInteger.MaxValue);
    public static readonly int MaxComponents = (int) SignatureWidth;
    public const int InitialColumnCapacity = 64;
}