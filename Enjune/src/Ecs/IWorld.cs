using Enjune.Ecs.EcsType;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enjune.src.Ecs;

public interface IWorld
{
    public void AddEntity(EntityAssembly assembly);
    public void RemoveEntity(EntityAssembly assembly);
    public void Update();
}
