using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public interface ISystem
{
    Signature Signature { get; }
    void Update();
}