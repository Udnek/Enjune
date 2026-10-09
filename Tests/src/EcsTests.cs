using Enjune.Ecs;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Registering;
using FluentAssertions;

namespace Tests;

public class EcsTests
{
    private readonly record struct TestPosition(int X) : IComponent
    {
        public Identifier Id() => Identifier.Of(typeof(TestPosition).Assembly, nameof(TestPosition));
    }

    private readonly record struct TestVelocity(int X) : IComponent
    {
        public Identifier Id() => Identifier.Of(typeof(TestVelocity).Assembly, nameof(TestVelocity));
    }

    private readonly record struct TestTag(string Value) : IComponent
    {
        public Identifier Id() => Identifier.Of(typeof(TestTag).Assembly, nameof(TestTag));
    }

    [Fact]
    public void World_ShouldCreateEntitiesWithSequentialIds()
    {
        var world = new World();

        var first = world.AddEntity(new Entity.Assembly());
        var second = world.AddEntity(new Entity.Assembly());
        var third = world.AddEntity(new Entity.Assembly());

        first.Should().Be(new Entity(1));
        second.Should().Be(new Entity(2));
        third.Should().Be(new Entity(3));
    }

    [Fact]
    public void World_ShouldAddAndRetrieveEntityComponent()
    {
        var world = new World();
        var entity = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(42)));

        var component = world.GetEntityComponent<TestPosition>(entity);

        component.Should().NotBeNull();
        component.Should().Be(new TestPosition(42));
    }

    [Fact]
    public void World_ShouldRemoveEntityComponent()
    {
        var world = new World();
        var entity = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(10)).AddComponent(new TestVelocity(5)));

        world.RemoveEntityComponent<TestVelocity>(entity);

        world.GetEntityComponent<TestVelocity>(entity).Should().BeNull();
        world.GetEntityComponent<TestPosition>(entity).Should().Be(new TestPosition(10));
    }

    [Fact]
    public void World_ShouldUpdateComponentValueThroughModifier()
    {
        var world = new World();
        var entity = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(4)));

        world.ModifyEntityComponent<TestPosition>(entity, position => position with { X = position.X + 2 }).Should().BeTrue();
        world.GetEntityComponent<TestPosition>(entity).Should().Be(new TestPosition(6));
    }

    [Fact]
    public void QueryBuilder_ShouldIncludeAndExcludeMatchingEntities()
    {
        var world = new World();
        var withPositionOnly = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(1)));
        var withPositionAndVelocity = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(2)).AddComponent(new TestVelocity(9)));
        var withVelocityOnly = world.AddEntity(new Entity.Assembly().AddComponent(new TestVelocity(4)));

        var matches = new List<Entity>();
        new QueryBuilder(world)
            .Including<TestPosition>()
            .Excluding<TestVelocity>()
            .Retrieve()
            .ForEach(entity => matches.Add(entity));

        matches.Should().HaveCount(1);
        matches.Should().Contain(withPositionOnly);
        matches.Should().NotContain(withPositionAndVelocity);
        matches.Should().NotContain(withVelocityOnly);
    }

    [Fact]
    public void World_ShouldMoveEntitiesAcrossArchetypes_WhenComponentsAreAddedAndRemoved()
    {
        var world = new World();
        var first = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(10)));
        var second = world.AddEntity(new Entity.Assembly()
            .AddComponent(new TestPosition(20))
            .AddComponent(new TestVelocity(5)));
        var third = world.AddEntity(new Entity.Assembly().AddComponent(new TestTag("initial")));

        world.AddEntityComponent<TestVelocity>(first, new TestVelocity(1));
        world.AddEntityComponent<TestTag>(first, new TestTag("leader"));

        world.RemoveEntityComponent<TestPosition>(second);
        world.AddEntityComponent<TestTag>(second, new TestTag("moved"));

        world.AddEntityComponent<TestPosition>(third, new TestPosition(30));
        world.RemoveEntityComponent<TestTag>(first);

        world.GetEntityComponent<TestPosition>(first).Should().Be(new TestPosition(10));
        world.GetEntityComponent<TestVelocity>(first).Should().Be(new TestVelocity(1));
        world.GetEntityComponent<TestTag>(first).Should().BeNull();

        world.GetEntityComponent<TestPosition>(second).Should().BeNull();
        world.GetEntityComponent<TestVelocity>(second).Should().Be(new TestVelocity(5));
        world.GetEntityComponent<TestTag>(second).Should().Be(new TestTag("moved"));

        world.GetEntityComponent<TestPosition>(third).Should().Be(new TestPosition(30));
        world.GetEntityComponent<TestTag>(third).Should().Be(new TestTag("initial"));

        var positionEntities = new List<Entity>();
        new QueryBuilder(world)
            .Including<TestPosition>()
            .Retrieve()
            .ForEach(entity => positionEntities.Add(entity));

        positionEntities.Should().Contain(new[] { first, third });
        positionEntities.Should().HaveCount(2);
        world.GetAllEntities().Should().HaveCount(3);
    }

    [Fact]
    public void World_ShouldKeepEntityStateConsistent_AfterRepeatedComponentToggles()
    {
        var world = new World();
        var alpha = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(1)));
        var beta = world.AddEntity(new Entity.Assembly().AddComponent(new TestVelocity(2)));
        var gamma = world.AddEntity(new Entity.Assembly()
            .AddComponent(new TestPosition(3))
            .AddComponent(new TestVelocity(4))
            .AddComponent(new TestTag("base")));

        for (int i = 0; i < 3; i++)
        {
            world.AddEntityComponent<TestTag>(alpha, new TestTag($"alpha-{i}"));
            world.RemoveEntityComponent<TestTag>(alpha);

            world.AddEntityComponent<TestPosition>(beta, new TestPosition(10 + i));
            world.RemoveEntityComponent<TestPosition>(beta);

            world.ModifyEntityComponent<TestTag>(gamma, tag => tag with { Value = $"gamma-{i}" }).Should().BeTrue();
            world.RemoveEntityComponent<TestTag>(gamma);
            world.AddEntityComponent<TestTag>(gamma, new TestTag($"gamma-{i}"));
        }

        world.GetEntityComponent<TestPosition>(alpha).Should().Be(new TestPosition(1));
        world.GetEntityComponent<TestTag>(alpha).Should().BeNull();

        world.GetEntityComponent<TestVelocity>(beta).Should().Be(new TestVelocity(2));
        world.GetEntityComponent<TestPosition>(beta).Should().BeNull();

        world.GetEntityComponent<TestPosition>(gamma).Should().Be(new TestPosition(3));
        world.GetEntityComponent<TestTag>(gamma).Should().Be(new TestTag("gamma-2"));
        world.GetEntityComponent<TestVelocity>(gamma).Should().Be(new TestVelocity(4));

        var query = new List<Entity>();
        new QueryBuilder(world)
            .Including<TestPosition>()
            .Retrieve()
            .ForEach(entity => query.Add(entity));

        query.Should().Contain(new[] { alpha, gamma });
        query.Should().HaveCount(2);
    }

    [Fact]
    public void Query_ShouldReflectEntityChanges_AfterExcludingTag_ThenAddingTag()
    {
        var world = new World();
        var tagged = world.AddEntity(new Entity.Assembly()
            .AddComponent(new TestPosition(1))
            .AddComponent(new TestTag("ready")));
        var untagged = world.AddEntity(new Entity.Assembly().AddComponent(new TestPosition(2)));

        var query = new QueryBuilder(world)
            .Including<TestPosition>()
            .Excluding<TestTag>()
            .Retrieve();

        var before = new List<Entity>();
        query.ForEach(entity => before.Add(entity));
        before.Should().ContainSingle().Which.Should().Be(untagged);

        world.AddEntityComponent<TestTag>(untagged, new TestTag("added-after-query"));

        var after = new List<Entity>();
        query.ForEach(entity => after.Add(entity));

        after.Should().BeEmpty();
        world.GetEntityComponent<TestTag>(untagged).Should().Be(new TestTag("added-after-query"));
        world.GetEntityComponent<TestPosition>(untagged).Should().Be(new TestPosition(2));
    }

    [Fact]
    public void SystemManager_ShouldInvokeSystemLifecycleAndUpdates()
    {
        var world = new World();
        var system = new TrackingSystem();

        world.AddSystem(system);
        world.Update();
        world.Update();

        system.Initialized.Should().BeTrue();
        system.UpdateCount.Should().Be(2);
    }

    private sealed class TrackingSystem : ISystem
    {
        public bool Initialized { get; private set; }
        public int UpdateCount { get; private set; }

        public void OnInit(World world)
        {
            Initialized = true;
        }

        public void OnUpdate()
        {
            UpdateCount++;
        }
    }
}
