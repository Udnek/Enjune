using Enjune.Ecs.Component;

namespace Enjune.Ecs.EcsType;

public partial class Query(World world, Signature include, Signature exclude)
{
    private readonly World _world = world;
    private readonly Signature _include = include;
    private readonly Signature _exclude = exclude;
    
    public static Builder For(World world) => new(world);
    
    public sealed class Builder
    {
        private readonly World _world;
        private readonly Signature.Builder _includeBuilder;
        private readonly Signature.Builder _excludeBuilder;

        internal Builder(World world)
        {
            _world = world;
            _includeBuilder = new Signature.Builder(world);
            _excludeBuilder = new Signature.Builder(world);
        }

        public Builder With<T>() where T : IComponent
        {
            _includeBuilder.RegisterComponent<T>();
            return this;
        }

        public Builder Without<T>() where T : IComponent
        {
            _excludeBuilder.RegisterComponent<T>();
            return this;
        }

        public Query Build() => new(_world, _includeBuilder.Build(), _excludeBuilder.Build());
    }
}