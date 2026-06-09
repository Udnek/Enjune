using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public interface ISystem
{
    Signature Signature { get; }
    void Initialize(SignatureBuilder assignedBuilder);
    void Update(World world);
}