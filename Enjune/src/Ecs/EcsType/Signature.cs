using Enjune.Ecs.Manager;
using JetBrains.Annotations;

namespace Enjune.Ecs.EcsType;

public readonly record struct Signature
{
    public static readonly Signature Empty = new(0);
    //uint -> 32-bit set
    //ulong -> 64-bit set
    private readonly SignatureInteger _bitSet;
    
    public Signature(SignatureInteger bitSet) => _bitSet = bitSet;
    
    [Pure]
    public Signature Flip(int bitPosition) => new(_bitSet & ~( (SignatureInteger) 1 << bitPosition));
    [Pure]
    public Signature Set(int bitPosition) => new(_bitSet | (SignatureInteger) 1 << bitPosition);
    [Pure]
    public Signature Unset(int bitPosition) => new(_bitSet & ~( (SignatureInteger) 1 << bitPosition));

    public bool IsSet(int bitPosition) => (_bitSet & ( (SignatureInteger) 1 << bitPosition)) != 0;

    public bool Contains(Signature other) => (_bitSet & other._bitSet) == other._bitSet;
    
    public bool Matches(Signature other) => _bitSet == other._bitSet;
    
    public int GetSetBitsCount()
    {
        int cnt = 0;
        SignatureInteger bitSetCopy = _bitSet;
        while (bitSetCopy > 0)
        {
            cnt += (int) bitSetCopy & 1;
            bitSetCopy >>= 1;
        }
        return cnt;
    }

    public override string ToString() => Convert.ToString(_bitSet, 2);
}

public class SignatureBuilder
{
    private readonly ComponentManager _componentManager;
    private Signature _signature = Signature.Empty;

    public SignatureBuilder(World world)
    {
        _componentManager = world.ComponentManager;
    }
    
    public SignatureBuilder RegisterComponent<T>()
    {
        var bit = (int)_componentManager.GetTypeIdByType(typeof(T));
        var sig = _signature;
        _signature = _signature.Set(bit);
        return this;
    }

    public SignatureBuilder RegisterComponent(Type type)
    {
        var bit = (int)_componentManager.GetTypeIdByType(type);
        _signature = _signature.Set(bit);
        return this;
    }

    public Signature Build() => _signature;
}