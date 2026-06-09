using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public interface ISystem
{
    void Initialize(SignatureBuilder assignedBuilder);
    void Update(World world);
}