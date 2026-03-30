using System.Collections;

namespace Enjune.Physics.Type;

public class Signature
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
}