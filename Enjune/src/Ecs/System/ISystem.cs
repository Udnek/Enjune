using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public interface ISystem
{
    void OnInit(World world);
    void OnUpdate(World world);
}