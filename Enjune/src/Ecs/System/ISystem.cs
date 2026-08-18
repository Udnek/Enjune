using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public interface ISystem
{
    void InitializeQueries(World world);
    void Update(World world);
}