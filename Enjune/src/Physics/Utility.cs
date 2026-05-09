using System.Collections;
using System.ComponentModel;
using Enjune.Physics.EcsType;

namespace Enjune.Physics;

// TODO: Move to Signature
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