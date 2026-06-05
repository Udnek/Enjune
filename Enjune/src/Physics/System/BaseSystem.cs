using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public abstract class BaseSystem : ISystem
{
    public Signature Signature { get; protected set; }
    public abstract void Initialize(SignatureBuilder assignedBuilder);
    public abstract void Update(World world);
}