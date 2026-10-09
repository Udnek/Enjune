using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Registering;
using FluentAssertions;

namespace Tests;

public class MinimalMinecraftLikeEcsTests
{
    private readonly record struct ChunkData(int ChunkId, bool ToBeUnloaded = false) : IComponent
    {
        public Identifier Id() => Identifier.Of(typeof(ChunkData).Assembly, nameof(ChunkData));
    }

    private readonly record struct GraphicLink(Guid GraphicIdValue) : IComponent
    {
        public Identifier Id() => Identifier.Of(typeof(GraphicLink).Assembly, nameof(GraphicLink));
    }

    private readonly record struct RemovalMarker(bool IsActive = true) : IComponent
    {
        public Identifier Id() => Identifier.Of(typeof(RemovalMarker).Assembly, nameof(RemovalMarker));
    }

    [Fact]
    public void Query_ShouldReturnSameMatches_RegardlessOfComponentOrder()
    {
        var world = new World();

        var entity = world.AddEntity(new Entity.Assembly().AddComponent(new ChunkData(11)));
        var other = world.AddEntity(new Entity.Assembly().AddComponent(new ChunkData(22)));

        world.AddEntityComponent(entity, new GraphicLink(Guid.Parse("11111111-1111-1111-1111-111111111111"))).Should().BeTrue();
        world.AddEntityComponent(other, new GraphicLink(Guid.Parse("22222222-2222-2222-2222-222222222222"))).Should().BeTrue();

        var firstQuery = new QueryBuilder(world)
            .Retrieve<ChunkData, GraphicLink>();
        var secondQuery = new QueryBuilder(world)
            .Retrieve<GraphicLink, ChunkData>();

        var orderedByChunkThenLink = new List<(Entity Entity, int ChunkId, Guid GraphicId)>();
        firstQuery.ForEach((entityId, ref chunk, ref link) =>
        {
            orderedByChunkThenLink.Add((entityId, chunk.ChunkId, link.GraphicIdValue));
        });

        var orderedByLinkThenChunk = new List<(Entity Entity, int ChunkId, Guid GraphicId)>();
        secondQuery.ForEach((entityId, ref link, ref chunk) =>
        {
            orderedByLinkThenChunk.Add((entityId, chunk.ChunkId, link.GraphicIdValue));
        });

        orderedByChunkThenLink.Should().HaveCount(2);
        orderedByLinkThenChunk.Should().HaveCount(2);
        orderedByChunkThenLink.Select(x => x.Entity.ToString()).Should().BeEquivalentTo([entity.ToString(), other.ToString()]);
        orderedByChunkThenLink.Select(x => x.ChunkId).Should().BeEquivalentTo([11, 22]);
        orderedByLinkThenChunk.Select(x => x.Entity.ToString()).Should().BeEquivalentTo([entity.ToString(), other.ToString()]);
        orderedByLinkThenChunk.Select(x => x.ChunkId).Should().BeEquivalentTo([11, 22]);
    }

    [Fact]
    public void World_ShouldRepeatMinecraftLikeChunkLifecycle_AcrossFrames()
    {
        var world = new World();
        var graphicObjects = new Dictionary<Guid, string>();
        var activeChunks = new HashSet<int>();
        var chunksToUnload = new HashSet<int>();

        var keepAlive = world.AddEntity(new Entity.Assembly()
            .AddComponent(new ChunkData(10, false))
            .AddComponent(new GraphicLink(Guid.NewGuid())));

        var link = world.GetEntityComponent<GraphicLink>(keepAlive)!.Value;
        graphicObjects[link.GraphicIdValue] = "model-10";
        activeChunks.Add(10);

        var marker = new ChunkUnloadMarkerSystem(chunksToUnload);
        marker.OnInit(world);
        var addModel = new ChunkAddModelSystem(graphicObjects, activeChunks);
        addModel.OnInit(world);
        var unload = new ChunkUnloadSystem(activeChunks, world);
        unload.OnInit(world);
        var removeModel = new ChunkRemoveModelSystem(graphicObjects, world);
        removeModel.OnInit(world);
        var remove = new EntityRemoveSystem(world);
        remove.OnInit(world);

        world.GetEntityComponent<ChunkData>(keepAlive).Should().Be(new ChunkData(10, false));
        world.GetEntityComponent<GraphicLink>(keepAlive).Should().NotBeNull();

        for (var chunkId = 20; chunkId < 25; chunkId++)
        {
            var entity = world.AddEntity(new Entity.Assembly().AddComponent(new ChunkData(chunkId)));
            activeChunks.Add(chunkId);

            addModel.OnUpdate();
            var addedLink = world.GetEntityComponent<GraphicLink>(entity)!.Value;
            graphicObjects.Should().ContainKey(addedLink.GraphicIdValue);
            graphicObjects[addedLink.GraphicIdValue].Should().Be($"model-{chunkId}");
            activeChunks.Should().Contain(chunkId);

            chunksToUnload.Add(chunkId);
            marker.OnUpdate();
            unload.OnUpdate();
            unload.MarkerAddResults.Should().HaveCount(chunkId - 19);
            unload.MarkerAddResults.Should().OnlyContain(result => result);
            world.GetEntityComponent<ChunkData>(entity).Should().Be(new ChunkData(chunkId, true));
            world.GetEntityComponent<RemovalMarker>(entity).Should().Be(new RemovalMarker());
            activeChunks.Should().NotContain(chunkId);

            removeModel.OnUpdate();
            graphicObjects.Should().NotContainKey(addedLink.GraphicIdValue);
            world.GetEntityComponent<GraphicLink>(entity).Should().BeNull();

            addModel.OnUpdate();
            world.GetEntityComponent<GraphicLink>(entity).Should().BeNull();

            remove.OnUpdate();
            world.GetAllEntities().Should().ContainSingle();
            world.GetEntityComponent<GraphicLink>(keepAlive).Should().NotBeNull();
            graphicObjects.Should().ContainKey(link.GraphicIdValue);
            activeChunks.Should().Contain(10);
        }
    }

    private sealed class ChunkUnloadMarkerSystem(ISet<int> chunksToUnload) : ISystem
    {
        private Query<ChunkData> _query = null!;

        public void OnInit(World world)
        {
            _query = new QueryBuilder(world).Retrieve<ChunkData>();
        }

        public void OnUpdate()
        {
            _query.ForEach((_, ref chunk) =>
            {
                if (chunksToUnload.Remove(chunk.ChunkId))
                    chunk = chunk with { ToBeUnloaded = true };
            });
        }
    }

    private sealed class ChunkAddModelSystem(
        IDictionary<Guid, string> graphicObjects,
        ISet<int> activeChunks) : ISystem
    {
        private Query<ChunkData> _query = null!;
        private World _world = null!;

        public void OnInit(World world)
        {
            _world = world;
            _query = new QueryBuilder(world)
                .Excluding<GraphicLink>()
                .Retrieve<ChunkData>();
        }

        public void OnUpdate()
        {
            _query.ForEach((entity, ref chunk) =>
            {
                if (chunk.ToBeUnloaded) return;

                var graphicId = Guid.NewGuid();
                _world.AddEntityComponent(entity, new GraphicLink(graphicId));
                graphicObjects[graphicId] = $"model-{chunk.ChunkId}";
                activeChunks.Add(chunk.ChunkId);
            });
        }
    }

    private sealed class ChunkRemoveModelSystem(
        IDictionary<Guid, string> graphicObjects,
        World world) : ISystem
    {
        private Query<ChunkData, GraphicLink> _query = null!;

        public void OnInit(World ecs)
        {
            _query = new QueryBuilder(ecs)
                .Retrieve<ChunkData, GraphicLink>();
        }

        public void OnUpdate()
        {
            _query.ForEach((entity, ref chunk, ref graphicLink) =>
            {
                if (!chunk.ToBeUnloaded) return;

                graphicObjects.Remove(graphicLink.GraphicIdValue);
                world.RemoveEntityComponent<GraphicLink>(entity);
            });
        }
    }

    private sealed class ChunkUnloadSystem(
        ISet<int> activeChunks,
        World world) : ISystem
    {
        private Query<ChunkData> _query = null!;
        public List<bool> MarkerAddResults { get; } = [];

        public void OnInit(World ecs)
        {
            _query = new QueryBuilder(ecs)
                .Excluding<RemovalMarker>()
                .Retrieve<ChunkData>();
        }

        public void OnUpdate()
        {
            _query.ForEach((entity, ref chunk) =>
            {
                if (!chunk.ToBeUnloaded) return;

                activeChunks.Remove(chunk.ChunkId);
                MarkerAddResults.Add(world.AddEntityComponent(entity, new RemovalMarker()));
            });
        }
    }

    private sealed class EntityRemoveSystem(World world) : ISystem
    {
        private Query<RemovalMarker> _query = null!;

        public void OnInit(World ecs)
        {
            _query = new QueryBuilder(ecs)
                .Excluding<GraphicLink>()
                .Retrieve<RemovalMarker>();
        }

        public void OnUpdate()
        {
            _query.ForEach((entity, ref _) =>
            {
                world.RemoveEntity(entity);
            });
        }
    }
}
