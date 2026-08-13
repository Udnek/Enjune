using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public interface ISystem
{
    void Initialize(Query preparedQuery);
    void Update(World world);
}