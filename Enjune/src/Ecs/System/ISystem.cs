using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public interface ISystem
{
    void Initialize(Signature.Builder assignedBuilder);
    void Update(World world);
}