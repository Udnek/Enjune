namespace Enjune.Physics;

using OpenTK.Mathematics;

public static class EcsConstants
{
    public const EntityId MaxEntities = 8192;
    public const ComponentTypeId MaxComponents = ComponentTypeId.MaxValue;
    public static readonly SignatureInteger SignatureWidth = (SignatureInteger) Math.Log2(SignatureInteger.MaxValue);
}