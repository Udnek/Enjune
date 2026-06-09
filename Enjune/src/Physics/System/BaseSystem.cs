using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public abstract class BaseSystem : ISystem
{
    protected Signature Signature;
    
    public void Initialize(SignatureBuilder assignedBuilder) 
        => Signature = GenerateSignature(assignedBuilder);

    protected abstract Signature GenerateSignature(SignatureBuilder builder);
    public abstract void Update(World world);
}