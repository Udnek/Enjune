using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Enjune;
using Enjune.Data;
using Enjune.Data.Json;
using Enjune.File;
using Enjune.Misc;
using Enjune.World;

namespace SceneMaker;

class Program
{
    private static void Main(string[] args)
    {
        // var quaternion = Quaternion.Identity;
        // quaternion.X = 42;
        // quaternion.W = 0;
        // Console.WriteLine(quaternion);
        // var json = JsonSerde.Tight.Serialize(Codecs.Quaternion.Encode(quaternion));
        // Console.WriteLine(json);
        // var obj = JsonSerde.Tight.Deserialize(json, out var error)!;
        // Console.WriteLine(Codecs.Quaternion.Decode(obj));


        var path = AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "papyrus.ttf"); //ExternalPath.Of("./", "aboba", "kek"); //
        var serialize = JsonSerde.Indent4.Serialize(ResourcePath.Codec.Encode(path));
        Logger.Highlight(typeof(Program),  '\n'+serialize);
        
        var obj = JsonSerde.Tight.Deserialize(serialize, out var error)!;
        Logger.Highlight(typeof(Program), ResourcePath.Codec.Decode(obj));

        Logger.Highlight(typeof(Program), typeof(Program).Assembly.GetName().Name);

        // Enjune.Enjune.Run(new App(), args);
    }
}