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
        // var s = new JsonOps().Serialize(
        //     new IDataObject.Map(new Dictionary<string, IDataObject>
        //     {
        //         {"k1", new IDataObject.Array([new IDataObject.Boolean(true), new IDataObject.String(null)])},
        //         {"k2", new IDataObject.String("kek")}
        //     }));

        //var json = "{\n    \"glossary\": {\n        \"title\": \"example glossary\",\n\t\t\"GlossDiv\": {\n            \"title\": \"S\",\n\t\t\t\"GlossList\": {\n                \"GlossEntry\": {\n                    \"ID\": \"SGML\",\n\t\t\t\t\t\"SortAs\": \"SGML\",\n\t\t\t\t\t\"GlossTerm\": \"Standard Generalized Markup Language\",\n\t\t\t\t\t\"Acronym\": \"SGML\",\n\t\t\t\t\t\"Abbrev\": \"ISO 8879:1986\",\n\t\t\t\t\t\"GlossDef\": {\n                        \"para\": \"A meta-markup language, used to create markup languages such as DocBook.\",\n\t\t\t\t\t\t\"GlossSeeAlso\": [\"GML\", \"XML\"]\n                    },\n\t\t\t\t\t\"GlossSee\": \"markup\"\n                }\n            }\n        }\n    }\n}";
        // var json = "[5]";
        // var data = JsonSerde.Indent4.Deserialize(json, out var error);
        // if (data == null)
        // {
        //     Logger.Error(typeof(Program), error);
        //     return;
        // }
        //
        // //Logger.Log(typeof(Program), $"bef: \n{json}");
        // Logger.Log(typeof(Program), $"aft: \n{JsonSerde.Tight.Serialize(data)}");

        // var data = JsonSerde.Tight.Deserialize("{\"y\": 5}", out var error);
        // if (data is null)
        // {
        //     Console.WriteLine(error);
        //     return;
        // }
        //
        // var vec = Codecs.Vector3.Decode(data);
        // Console.WriteLine(vec);
        // //Console.WriteLine(JsonSerde.Indent4.Serialize(data));
        // var serialized = JsonSerde.Indent4.Serialize(Codecs.Vector3.Encode(vec));
        // Console.WriteLine(serialized);

        // ExternalPath.Of("./", "test.json").Write(serialized, out var error1);
        // Console.WriteLine(error1);

        // var s = JsonSerde.Instance.Serialize(Codec.Vector3.Encode(new Vector3(1, 5, 2.3f)));
        // Console.WriteLine(s);

        //Logger.Highlight(typeof(Program),  Deep EEquals(new int[0], new int[0]));
        //Logger.Highlight(typeof(Program), '\n'+JsonSerde.Indent4.Serialize(Scene.Codec.Encode(new Scene())));
        
        // Enjune.Enjune.Run(new App(), args);
    }
}