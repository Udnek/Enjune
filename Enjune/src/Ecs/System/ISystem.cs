using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public interface ISystem
{
    void Initialize(SignatureBuilder assignedBuilder);
    void Update(World world);
}