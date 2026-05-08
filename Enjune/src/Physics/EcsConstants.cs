namespace Enjune.Physics;

using OpenTK.Mathematics;

public static class EcsConstants
{
    public const EntityId MaxEntities = 8192;
    public static readonly SignatureInteger SignatureWidth = (SignatureInteger) Math.Log2(SignatureInteger.MaxValue);
    public static readonly int MaxComponents = (int) SignatureWidth;
    public const int InitialCapacity = 64;
    public const float GravitationalAcceleration = -9.80665f;
    public const float DeltaTime = 0.01f;
}