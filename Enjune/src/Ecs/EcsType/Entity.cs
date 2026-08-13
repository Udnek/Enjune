using Enjune.Ecs.Component;
using Enjune.Misc;
using System;
using System.Text;

namespace Enjune.Ecs.EcsType;

public readonly struct Entity
{
    public readonly uint Id;

    public Entity(uint id) => Id = id;

    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"Entity[id = {Id}]";

    public class Snapshot(Entity entity)
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

    //public class Assembly
    //{
    //    private readonly Dictionary<Type, IComponent> _components = new();

    //    public List<IComponent> GetComponents() => _components.Values.ToList();
    //    public List<Type> GetComponentTypes() => _components.Keys.ToList();

    //    public Entity.Assembly AddComponent(IComponent component)
    //    {
    //        if (_components.ContainsKey(component.GetType()))
    //            Logger.Warn(this, $"Replaced component {component.GetType()} for an assembly");
    //        _components.Add(component.GetType(), component);

    //        return this;
    //    }

    //    public void RemoveComponent(IComponent component)
    //        => _components.Remove(component.GetType());

    //    public IComponent? GetComponent(Type componentType)
    //    {
    //        _components.TryGetValue(componentType, out IComponent? component);
    //        return component;
    //    }

    //    public Signature GetSignature(World world)
    //    {
    //        var builder = new Signature.Builder(world);
    //        foreach (Type componentType in GetComponentTypes())
    //            builder.RegisterComponent(componentType);
    //        return builder.Build();
    //    }
    //}
}
