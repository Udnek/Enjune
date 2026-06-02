using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public interface ISystem
{
    Signature Signature { get; }
    // TODO method should probably take arg with all entities??? or make UpdateEntity() and run per each entity idk
    void Update();
}