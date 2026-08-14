using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

[Obsolete]
public class TrackingSystem : SingleQuerySystem
{
    //private EntityId _trackedEntity;

    //// TODO: Do we want it to track entity removal? 
    //public TrackingSystem(EntityId entityId)
    //{
    //    _trackedEntity = entityId;
    //}

    //protected override Signature GenerateSignature(Signature.Builder builder)
    //{
    //    return builder
    //        .RegisterComponent<Component.Position>()
    //        .RegisterComponent<Velocity>()
    //        .RegisterComponent<Acceleration>()
    //        .Build();
    //}

    //public override void Update(World world)
    //{

    //}
    protected override Query.State BuildQuery(Query preparedQuery)
    {
        throw new NotImplementedException();
    }
    public override void Update(World world)
    {
        throw new NotImplementedException();
    }
}