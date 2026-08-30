using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
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

//public delegate void ForEachDelegate<T1, T2>(ref T1 c1, ref T2 c2) 
//    where T1 : struct, IComponent 
//    where T2 : struct, IComponent;

//public partial class Query
//{
//    public void ForEach<T1, T2>(ForEachDelegate<T1, T2> action) 
//        where T1 : struct, IComponent 
//        where T2 : struct, IComponent
//    {
//        var cache = Cache<T1, T2>.GetColumns(_world, _include, _exclude);
//        foreach (var (col1,  col2) in cache)
//        {
//            for (int i = 0; i < col1.Count; i++)
//            {
//                action(ref col1[i], ref col2[i]);
//            }
//        }
//    }

//    private static class Cache<T1, T2>
//        where T1 : struct, IComponent
//        where T2 : struct, IComponent
//    {
//        private static List<(Column<T1> Col1, Column<T2> Col2)> _columns = [];
//        private static int _cacheVersion = -1;

//        internal static List<(Column<T1> Col1, Column<T2> Col2)> GetColumns(
//            World world, Signature include, Signature exclude)
//        {
//            if (world.CacheVersion == _cacheVersion) return _columns;
//            _columns.Clear();
//            foreach (var archetype in world.QueryArchetypes(include, exclude))
//            {
//                _columns.Add((archetype.GetColumn<T1>(), archetype.GetColumn<T2>()));
//            }
//            _cacheVersion = world.CacheVersion;

//            return _columns;
//        }
//    }
//}

[Obsolete]
public sealed class OldQuery(World world, Signature include, Signature exclude)
{
    private readonly World _world = world;
    private readonly Signature _include = include;
    private readonly Signature _exclude = exclude;

    private readonly List<Archetype> _cachedArchetypes = [];
    private int _cacheVersion = 0;

    private void ValidateCache()
    {
        if (_world.CacheVersion == _cacheVersion)
            return;

        _cacheVersion = _world.CacheVersion;
        _cachedArchetypes.Clear();
        foreach (var archetype in _world.QueryArchetypes(_include, _exclude))
            _cachedArchetypes.Add(archetype);
    }

    public void ForEachArchetype(Action<Archetype> action)
    {
        ValidateCache();
        foreach (var archetype in _cachedArchetypes)
            action(archetype);
    }

    public static Builder For(World world) => new(world);

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

        public OldQuery Build() => new(_world, _includeBuilder.Build(), _excludeBuilder.Build());
    }
}