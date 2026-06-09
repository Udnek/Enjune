using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

public class TrackingSystem : BaseSystem
{
    private EntityId _trackedEntity;

    // TODO: Do we want it to track entity removal? 
    public TrackingSystem(EntityId entityId)
    {
        _trackedEntity = entityId;
    }

    protected override Signature GenerateSignature(SignatureBuilder builder)
    {
        return builder
            .RegisterComponent<Component.Position>()
            .RegisterComponent<Velocity>()
            .RegisterComponent<Acceleration>()
            .Build();
    }

    public override void Update(World world)
    {
        
    }
}