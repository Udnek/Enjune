using Enjune.Ecs.Component;
using Enjune.Misc;
using System;
using System.Text;

namespace Enjune.Ecs.EcsType;

public readonly struct Entity
{
    private readonly uint _id;

    public Entity(uint id) => _id = id;
    public Entity(int id) => _id = (uint)id;
    public override string ToString() => $"Entity {_id}";

    public class Assembly()
    {
        private readonly Dictionary<Type, IComponent> _components = new();

        public IEnumerable<IComponent> GetComponents() => _components.Values;
        public IEnumerable<Type> GetComponentTypes() => _components.Keys;

        public Assembly AddComponent(IComponent component)
        {
            if (_components.ContainsKey(component.GetType()))
                Logger.Warn(this, $"Replaced component {component.GetType()} for an assembly");
            _components.Add(component.GetType(), component);

            return this;
        }

        public void RemoveComponent(IComponent component)
            => _components.Remove(component.GetType());

        public IComponent? GetComponent(Type componentType)
        {
            _components.TryGetValue(componentType, out IComponent? component);
            return component;
        }

        public Signature GetSignature(World world)
        {
            var builder = new Signature.Builder(world);
            foreach (Type componentType in GetComponentTypes())
                builder.RegisterComponent(componentType);
            return builder.Build();
        }
    }
}
