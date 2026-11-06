using System.Diagnostics.Contracts;
using Enjune.Ecs.Manager;

namespace Enjune.Ecs.EcsType;

/// <summary>
/// <para>A set of bits with the same length as <see cref="SignatureInteger"/>,<br/>
/// where each bit is meant to represent a <c>true</c>/<c>false</c> value.</para>
/// </summary>
public readonly record struct Signature
{
    public static readonly Signature Empty = new(0);
    public static readonly Signature Full = new(1);
    //uint -> 32-bit set
    //ulong -> 64-bit set
    private readonly SignatureInteger _bitSet;
    
    public Signature(SignatureInteger bitSet) => _bitSet = bitSet;
    
    [Pure]
    public Signature Flip(int bitPosition) => new(_bitSet & ~( 1 << bitPosition));
    [Pure]
    public Signature Set(int bitPosition) => new(_bitSet | 1 << bitPosition);
    [Pure]
    public Signature Unset(int bitPosition) => new(_bitSet & ~( 1 << bitPosition));
    
    public bool IsSet(int bitPosition) => (_bitSet & ( 1 << bitPosition)) != 0;

    public bool Includes(Signature other) => (_bitSet & other._bitSet) == other._bitSet;

    public bool Excludes(Signature other) => (~other._bitSet & _bitSet) == _bitSet;
    
    public Signature Or(Signature other) => new(_bitSet | other._bitSet);
    
    public int GetSetBitsCount()
    {
        int cnt = 0;
        SignatureInteger bitSetCopy = _bitSet;
        while (bitSetCopy > 0)
        {
            cnt += bitSetCopy & 1;
            bitSetCopy >>= 1;
        }
        return cnt;
    }

    public override string ToString() => Convert.ToString(_bitSet, 2).PadLeft(8, '0');

    /// <summary>
    /// Allows to build a <see cref="Signature"/> using <paramref name="world"/> as context.<br/>
    /// Resulting <see cref="Signature"/> would have a bit <c>n</c> set, if a component<br/>
    /// with a type id of <c>n</c> in <paramref name="world"/> was added in the process of building.
    /// </summary>
    /// <param name="world"></param>
    public sealed class Builder(World world)
    {
        private readonly ComponentManager _componentManager = world.ComponentManager;
        private readonly List<Type> _componentTypes = [];

        public Builder RegisterComponent<T>() => RegisterComponent(typeof(T));

        public Builder RegisterComponent(Type type)
        {
            _componentTypes.Add(type);
            return this;
        }

        public Signature Build()
        {
            var signature = Empty;
            foreach (var type in _componentTypes)
            {
                var bit = _componentManager.GetIdByType(type);
                signature = signature.Set(bit);
            }
            return signature;
        }
    }
}
