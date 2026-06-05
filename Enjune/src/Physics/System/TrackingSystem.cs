using System.Runtime.InteropServices;
using Enjune.Physics.Component;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public class TrackingSystem : BaseSystem
{
    private EntityId _trackedEntity;

    // TODO: Do we want it to track entity removal? 
    public TrackingSystem(EntityId entityId)
    {
        _trackedEntity = entityId;
    }

    public override void Initialize(SignatureBuilder assignedBuilder)
    {
        Signature = assignedBuilder
            .RegisterComponent<Physics.Component.Position>()
            .RegisterComponent<Velocity>()
            .RegisterComponent<Acceleration>()
            .Build();
    }

    public override void Update(World world)
    {
        throw new NotImplementedException();
    }
}