using Enjune.Data;
using Enjune.Registering;

namespace Enjune.Graphic.Modeling;

public sealed class RegistrableModel
{
    public static readonly Registry<RegistrableModel> Registry = new();
    
    public static readonly Codec<RegistrableModel?> Codec = Codecs.ForRegistryEntry(Registry, i => i.Id);
    
    public static RegistrableModel CreateAndRegister(Identifier id, Model model, bool registerAsDefault = false)
    {
        var registrableModel = new RegistrableModel(id, model);
        return Registry.Register(id, registrableModel, registerAsDefault);
    }
    
    public Model Model { get; }
    public Identifier Id { get; }
    
    private RegistrableModel(Identifier id, Model model)
    {
        Model = model;
        Id = id;
    }
}