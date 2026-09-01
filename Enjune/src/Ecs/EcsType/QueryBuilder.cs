using Enjune.Ecs.Component;

namespace Enjune.Ecs.EcsType;

public sealed partial class QueryBuilder(World world)
{
    private readonly World _world = world;
    private readonly Signature.Builder _includeBuilder = new(world);
    private readonly Signature.Builder _excludeBuilder = new(world);

    public QueryBuilder Including<T>() where T : struct, IComponent
    {
        _includeBuilder.RegisterComponent<T>();
        return this;
    }

    public QueryBuilder Excluding<T>() where T : struct, IComponent
    {
        _excludeBuilder.RegisterComponent<T>();
        return this;
    }
}