using System.Collections;
using Enjune.Misc;

namespace Enjune.Physics.EcsType;

public record struct Signature
{
    //uint -> 32-bit set
    //ulong -> 64-bit set
    private SignatureInteger _bitSet;
    
    public Signature(SignatureInteger bitSet) => _bitSet = bitSet;
    
    public void Flip(int bitPosition) => _bitSet &= ~( (SignatureInteger) 1 << bitPosition);

    public void Set(int bitPosition) => _bitSet |= (SignatureInteger) 1 << bitPosition;

    public void Unset(int bitPosition) => _bitSet &= ~( (SignatureInteger) 1 << bitPosition);

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

    public override string ToString()
    {
        return Convert.ToString(_bitSet, 2);
    }
}

public class SignatureBuilder
{
    private Signature _signature = new Signature(0);
    public SignatureBuilder RegisterComponent<T>()
    {
        var bit = (int)World.ComponentManager.GetTypeIdByType(typeof(T));
        _signature.Set(bit);
        return this;
    }

    public SignatureBuilder RegisterComponent(Type type)
    {
        var bit = (int)World.ComponentManager.GetTypeIdByType(type);
        _signature.Set(bit);
        return this;
    }

    public Signature Build()
    {
        return _signature;
    }
}