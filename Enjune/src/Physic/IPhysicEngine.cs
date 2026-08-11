namespace Enjune.Physic;

public interface IPhysicEngine : IDisposable
{
    IPhysicWorld CreateWorld();
}