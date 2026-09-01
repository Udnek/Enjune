using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Net;
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

    public void ForEach(Action<Entity> action)
    {
        var cache = Cache.GetCache(_world, _include, _exclude);
        foreach (var entity in cache)
            action(entity);
    }
    private static class Cache
    {
        private static List<Entity> _cache = [];
        private static int _cacheVersion = -1;
        internal static List<Entity> GetCache(
            World world, Signature include, Signature exclude)
        {
            if (world.CacheVersion == _cacheVersion) return _cache;
            _cache.Clear();
            foreach (var archetype in world.QueryArchetypes(include, exclude))
            {
                var entities = archetype.GetEntities();
                for (int i = 0; i < archetype.Rows; i++)
                {
                    _cache.Add(entities[i]);
                }
            }
            _cacheVersion = world.CacheVersion;
            return _cache;
        }
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

        public Query Build() => new(_world, _includeBuilder.Build(), _excludeBuilder.Build());
    }
}
public sealed partial class QueryBuilder(World world)
{
    private readonly World _world = world;
    private readonly Signature.Builder _includeBuilder = new(world);
    private readonly Signature.Builder _excludeBuilder = new(world);

    public QueryBuilder IncludeWith<T>() where T : struct, IComponent
    {
        _includeBuilder.RegisterComponent<T>();
        return this;
    }

    public QueryBuilder ExcludeWith<T>() where T : struct, IComponent
    {
        _excludeBuilder.RegisterComponent<T>();
        return this;
    }
}

//public delegate void ForEachDelegate<T1, T2>(Entity entity, ref T1 t1, ref T2 t2) where T1 : struct, IComponent where T2 : struct, IComponent;

//public class Query<T1, T2>(World world, Signature include, Signature exclude) where T1 : struct, IComponent where T2 : struct, IComponent
//{
//    private readonly World _world = world;
//    private readonly Signature _include = include;
//    private readonly Signature _exclude = exclude;

//    private static List<(Entity[] entities, Column<T1> Column1, Column<T2> Column2)> _cache = [];
//    private static int _cacheVersion = -1;

//    public void ForEach(ForEachDelegate<T1, T2> action)
//    {
//        var cache = GetCache(_world, _include, _exclude);
//        foreach (var (entities, col1, col2) in cache)
//        {
//            for (int i = 0; i < col1.Count; i++)
//            {
//                action(entities[i], ref col1[i], ref col2[i]);
//            }
//        }
//    }

//    internal List<(Entity[] entities, Column<T1> Column1, Column<T2> Column2)> GetCache(
//            World world, Signature include, Signature exclude)
//    {
//        if (world.CacheVersion == _cacheVersion) return _cache;
//        _cache.Clear();
//        foreach (var archetype in world.QueryArchetypes(include, exclude))
//        {
//            _cache.Add((archetype.GetEntities(), archetype.GetColumn<T1>(), archetype.GetColumn<T2>()));
//        }
//        _cacheVersion = world.CacheVersion;
//        return _cache;
//    }
//}

//public partial class QueryBuilder
//{
//    public Query<T1, T2> Retrieve<T1, T2>() where T1 : struct, IComponent where T2 : struct, IComponent
//    {
//        return new Query<T1, T2>(_world, _includeBuilder.Build(), _excludeBuilder.Build());
//    }
//}