namespace Enjune.Ecs;

public static class EcsConstants
{
    public static readonly SignatureInteger SignatureWidth = (SignatureInteger) Math.Log2(SignatureInteger.MaxValue);
    public static readonly int MaxComponentsPerEntity = (int) SignatureWidth;
    public const int InitialColumnCapacity = 64;
}