using Enjune.Ecs;
using Enjune.Ecs.System;

namespace Minecraft2.Ecs.System;

public abstract class AppSystem : ISystem
{
    public required App App;
    public abstract void OnInit(World world);
    public abstract void OnUpdate();
}