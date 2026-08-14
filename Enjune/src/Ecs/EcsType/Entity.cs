using Enjune.Ecs.Component;
using Enjune.Misc;
using System;
using System.Text;

namespace Enjune.Ecs.EcsType;

public readonly record struct Entity
{
    private readonly uint _id;

    public Entity(uint id) => _id = id;
    public Entity(int id) => _id = (uint)id;

    public sealed class Snapshot(Entity entity)
    {
        public readonly Entity Entity = entity;
        private readonly Dictionary<Type, IComponent> _components = new();
        
        public Snapshot AddComponent(IComponent component)
        {
            if (_components.ContainsKey(component.GetType()))
                Logger.Warn(this, $"Replaced component {component.GetType()} for a snapshot. Component collision should not happen");
            _components.Add(component.GetType(), component);

            return this;
        }
        public void RemoveComponent(IComponent component)
            => _components.Remove(component.GetType());

        public List<IComponent> GetComponents() => _components.Values.ToList();
        public List<Type> GetComponentTypes() => _components.Keys.ToList();
    }
}
