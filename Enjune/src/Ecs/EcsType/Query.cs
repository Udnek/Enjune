using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Enjune.Ecs.EcsType;

public partial class Query(World world, Signature include, Signature exclude)
{
    private readonly World _world = world;
    private readonly Signature _include = include;
    private readonly Signature _exclude = exclude;



    public sealed class Builder(World world)
    {
        private readonly World _world = world;
        private readonly Signature.Builder _includeBuilder = new(world);
        private readonly Signature.Builder _excludeBuilder = new(world);

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

//[Obsolete]
//public sealed partial class Query(World world, Signature include, Signature exclude)
//{
//    private readonly World _world = world;
//    private readonly Signature _include = include;
//    private readonly Signature _exclude = exclude;

//    private readonly List<Archetype> _cachedArchetypes = [];
//    private int _cacheVersion = 0;

//    private void ValidateCache()
//    {
//        if (_world.CacheVersion == _cacheVersion)
//            return;

//        _cacheVersion = _world.CacheVersion;
//        _cachedArchetypes.Clear();
//        foreach (var archetype in _world.QueryArchetypes(_include, _exclude))
//            _cachedArchetypes.Add(archetype);
//    }

//    public void ForEachArchetype(Action<Archetype> action)
//    {
//        ValidateCache();
//        foreach (var archetype in _cachedArchetypes)
//            action(archetype);
//    }

//    public static Builder For(World world) => new(world);

//    public sealed class Builder(World world)
//    {
//        private readonly World _world = world;
//        private readonly Signature.Builder _includeBuilder = new(world);
//        private readonly Signature.Builder _excludeBuilder = new(world);

//        public Builder With<T>() where T : IComponent
//        {
//            _includeBuilder.RegisterComponent<T>();
//            return this;
//        }

//        public Builder Without<T>() where T : IComponent
//        {
//            _excludeBuilder.RegisterComponent<T>();
//            return this;
//        }

//        public Query Build() => new(_world, _includeBuilder.Build(), _excludeBuilder.Build());
//    }
//}