using Enjune.Data.Codec;
using Enjune.Registering;

namespace Enjune.Ecs.Component;

public interface IComponent
{
    Identifier Id();
}