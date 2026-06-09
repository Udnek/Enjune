using System.Reflection;
using Enjune.Data.Json;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Enjune.Registering;
using Enjune.World;

namespace SceneMaker;

public static class SceneManager
{
    private static readonly ExternalPath Path = ExternalPath.Of("scene.json");

    private static Assembly Assembly => typeof(SceneManager).Assembly;
    
    private static Error? LoadModels(AssetManager assetManager)
    {
        Models.ErrorCube = RegistrableModel.CreateAndRegister(new Identifier(Assembly, "error_cube"),
            new Model(Mesh.Cube(Position.Zero, 1f, TextureQuad.Full), new Model.PerMesh(assetManager.MissingMaterial)),
            true);
        
        var calaveraRawModel = new DotGlbReader()
            .Read(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly, "Models", "Calavera", "Calavera.glb"), out var error);
        if (calaveraRawModel == null) return error;
        Models.Calavera = RegistrableModel.CreateAndRegister(new Identifier(Assembly, "calavera"), calaveraRawModel);
        
        Models.WhiteCube = RegistrableModel.CreateAndRegister(new Identifier(Assembly, "white_cube"),
            new Model.Builder()
                .Add(Mesh.Cube(Position.Zero, 0.5f, TextureQuad.Full), new Model.PerMesh(assetManager.WhiteMaterial))
                .Build());

        return null;
    }
    
    public static (Scene? Scene, Error? Error) Load(AssetManager assetManager)
    {
        var error = LoadModels(assetManager);
        if (error != null) return (null, error);

        var json = Path.LoadText(out error);
        if (json == null)
        {
            Logger.Log(typeof(SceneManager), $"creating default scene because can not load from file: {error}");
            return CreateNewScene(assetManager);
        }

        var data = JsonSerde.Tight.Deserialize(json, out error);
        if (data == null)
        {
            Logger.Log(typeof(SceneManager), $"creating default scene because can not load from file: {error}");
            return CreateNewScene(assetManager);
        }

        var scene = Scene.Codec.Decode(data);
        Logger.Log(typeof(SceneManager), "successfully load scene from file");
        return (scene, null);
    }
    
    private static (Scene? Scene, Error? Error) CreateNewScene(AssetManager assetManager)
    {
        var scene = new Scene();
        
        // calavera

        scene.Objects.Add(new SObject()
        {
            Model = Models.Calavera,
        });
        
        // lights
        {
            var light = new SObject()
            {
                Model = Models.WhiteCube,
                Position = (0, 20, -25/2f),
                SpotLight = SpotLight.Ortho(new Vector3(0f, -1, -0.5f), new Color(244/255f, 233/255f, 200/255f, 1f)*1.5f, (30, 30))
            };
            scene.Objects.Add(light);
                
            scene.Objects.Add(new SObject()
            {
                Model = Models.WhiteCube,
                Position = (6, 4, 0),
                SpotLight = SpotLight.Perspective(-Vector3.UnitY, new Color(1, 1, 0, 1), 45f)
            });
        }
        return (scene, null);
    }

    public static void Save(Scene scene)
    {
        var json = JsonSerde.Indent4.Serialize(Scene.Codec.Encode(scene));
        Path.Write(json, out var error);
        if (error != null) 
            Logger.Log(typeof(SceneManager), $"can not save scene: {error}");
    }
}