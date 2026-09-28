using System.Reflection;
using Enjune.Graphic.Modeling;
using Enjune.Registering;

namespace SceneMaker.Misc;

public static class Models
{
    public static readonly WritableRegistry<StaticModel> Registry = WritableRegistry<StaticModel>
        .CreateAndRegister(Identifier.Of(Program.Assembly, "model"));

    public static readonly RegistryReference<StaticModel> ErrorCube = Create("error_cube");
    public static readonly RegistryReference<StaticModel> Calavera = Create("calavera");
    public static readonly RegistryReference<StaticModel> WhiteCube = Create("white_cube");
    
    private static RegistryReference<StaticModel> Create(string name) 
        => Registry.CreateReference(Identifier.Of(Program.Assembly, name));
}