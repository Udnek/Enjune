using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public abstract class BaseSystem : ISystem
{
    protected Signature Signature { get; private set; }

    public void Initialize(Signature.Builder assignedBuilder) 
        => Signature = GenerateSignature(assignedBuilder);

    protected abstract Signature GenerateSignature(Signature.Builder builder);
    public abstract void Update(World world);
}