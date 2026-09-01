namespace Enjune.Ecs.System;

public interface ISystem
{
    void OnInit(World world);
    // Is passing down World necessary?
    void OnUpdate(World world);
}