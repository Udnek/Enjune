namespace Enjune.Ecs.System;

public interface ISystem
{
    void OnInit(World world);
    void OnUpdate();
}