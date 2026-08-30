using System.Reflection;
using Enjune.Graphic.Modeling;
using Enjune.Registering;

namespace SceneMaker.Misc;

public static class Models
{
    public static readonly WritableRegistry<Model> Registry = WritableRegistry<Model>.CreateAndRegister(Identifier.Of(Program.Assembly, "model"));

    public static readonly RegistryReference<Model> ErrorCube = Create(Program.Assembly, "error_cube");
    public static readonly RegistryReference<Model> Calavera = Create(Program.Assembly, "calavera");
    public static readonly RegistryReference<Model> WhiteCube = Create(Program.Assembly, "white_cube");
    
    private static RegistryReference<Model> Create(Assembly assembly, string name) 
        => Registry.CreateReference(Identifier.Of(assembly, name));
}