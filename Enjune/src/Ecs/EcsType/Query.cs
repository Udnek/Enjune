using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Enjune.Ecs.EcsType;

public class Query(World World)
{
    private readonly World _world = World;

    public class State(Signature IncludeSignature, Signature ExcludeSignature, World World)
    {
        private readonly Signature _includeSignature = IncludeSignature;
        private readonly Signature _excludeSignature = ExcludeSignature;
        private readonly List<Type> _componentTypes = World.DeconstructSignature(IncludeSignature);
        private readonly World _world = World;

        private List<Archetype> _cachedArchetypes = [];
        private int _cacheVersion = 0;
        
        // TODO: Ensure cache invalidation is in place in World logic
        private void ValidateCache()
        {
            if (_world.ArchetypeCacheVersion == _cacheVersion) return;
            _cachedArchetypes = _world.GetMatchedArchetypes(_includeSignature, _excludeSignature);

            _cacheVersion = _world.ArchetypeCacheVersion;
        }

        public void ForEachArchetype(Action<Archetype> action)
        {
            ValidateCache();
            foreach (var archetype in _cachedArchetypes)
                action(archetype);
        }
    }

    public class Builder(Signature.Builder IncludeBuilder, Signature.Builder ExcludeBuilder, World World)
    {
        private readonly Signature.Builder _includeBuilder = IncludeBuilder;
        private readonly Signature.Builder _excludeBuilder = ExcludeBuilder;
        private readonly World _world = World;

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
        public State Build()
        {
            return new State(_includeBuilder.Build(), _excludeBuilder.Build(), _world);
        }
    }

    public Builder With<T>() where T : IComponent
    {
        return new Builder(new Signature.Builder(_world).RegisterComponent<T>(), new Signature.Builder(_world), _world);
    }

    public Builder Without<T>() where T : IComponent
    {
        return new Builder(new Signature.Builder(_world), new Signature.Builder(_world).RegisterComponent<T>(), _world);
    }
}