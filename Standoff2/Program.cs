using Enjune;
using System.Reflection;
using Enjune.Data.Codec;
using Enjune.Data.Json;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Enjune.Registering;

internal class Program
{

    public static Assembly Assembly => typeof(Program).Assembly;

    private static void Main(string[] args)
    {
        Logger.RegisterNamespaceToDomain(typeof(Program).Assembly, "", new Logger.Domain("SceneMaker", ConsoleColor.DarkRed));
        Enjune.Enjune.Run(new EcsTestApp(), args);

        // Logger.Info(typeof(Program), JsonSerde.Indent4.Serialize(ModelComponent.Codec.Encode(
        //     new ModelComponent 
        //     { 
        //         Model = Models.Calavera, 
        //         DropsShadow = true, 
        //         IsHidden = false 
        //     }
        //     )
        // ));
    }
}