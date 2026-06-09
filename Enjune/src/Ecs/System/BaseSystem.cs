using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public abstract class BaseSystem : ISystem
{
    protected Signature Signature { get; private set; }

    public void Initialize(SignatureBuilder assignedBuilder) 
        => Signature = GenerateSignature(assignedBuilder);

    protected abstract Signature GenerateSignature(SignatureBuilder builder);
    public abstract void Update(World world);
}