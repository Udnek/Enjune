using Enjune.Ecs.Component;
using Enjune.Misc;

namespace Enjune.Ecs.EcsType;

/// <summary>
/// <para>A symbolic object that contains an id,<br/>
/// and is essentially a key to getting<br/>
/// all data in the <see cref="World"/> that is assigned with that id.</para>
/// <para><see cref="Entity"/> can't be <c>0</c>. Such id is considered a <c>null entity</c>.</para>
/// </summary>
public readonly record struct Entity
{
    private readonly uint _id;

    public Entity(uint id) => _id = id;
    public Entity(int id) => _id = (uint)id;
    
    public override string ToString() => _id == 0 ? "Entity null" : $"Entity {_id}";

    /// <summary>
    /// A common OOP object that allows to store <see cref="IComponent"/> structs<br/>
    /// along with their actual types. 
    /// </summary>
    public sealed class Assembly()
    {
        private readonly Dictionary<Type, IComponent> _components = new();

        public IEnumerable<IComponent> GetComponents() => _components.Values;
        public IEnumerable<Type> GetComponentTypes() => _components.Keys;

        /// <summary>
        /// Adds an <see cref="IComponent"/> to the <see cref="Entity.Assembly"/>.<br/>
        /// If the same type as <paramref name="component"/> was added before, replaces the old one with <paramref name="component"/>.
        /// </summary>
        /// <param name="component"></param>
        /// <returns>The same instance of <see cref="Entity.Assembly"/> for chain building.</returns>
        public Assembly AddComponent(IComponent component)
        {
            if (_components.ContainsKey(component.GetType()))
                Logger.Logger.Warn(this, $"Replaced component {component.GetType()} for an assembly");
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

        /// <summary>
        /// Constructs a <i>new</i> <see cref="Signature"/> using <paramref name="world"/> as context.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public Signature GetSignature(World world)
        {
            var builder = new Signature.Builder(world);
            foreach (Type componentType in GetComponentTypes())
                builder.RegisterComponent(componentType);
            return builder.Build();
        }
    }
}
