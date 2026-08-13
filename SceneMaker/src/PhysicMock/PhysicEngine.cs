using Enjune.Misc;
using Enjune.Physic;

namespace SceneMaker.PhysicMock;

public class PhysicEngine : AbstractDisposable, IPhysicEngine
{
    public IPhysicWorld CreateWorld() => new PhysicWorld();

    protected override void DisposeData() { }
}