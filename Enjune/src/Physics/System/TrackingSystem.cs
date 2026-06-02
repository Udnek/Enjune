using System.Runtime.InteropServices;
using Enjune.Physics.Component;
using Enjune.Physics.EcsType;

namespace Enjune.Physics.System;

public class TrackingSystem : ISystem
{
    public Signature Signature { get; }
    private EntityId _trackedEntity;

    // TODO: Do we want it to track entity removal? 
    public TrackingSystem(EntityId entityId)
    {
        _trackedEntity = entityId;
        Signature = new SignatureBuilder()
            .RegisterComponent<Physics.Component.Position>()
            .RegisterComponent<Velocity>()
            .RegisterComponent<Acceleration>()
            .Build();
    }
    
    public void Update()
    {
        
    }
}