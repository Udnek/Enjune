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

namespace SceneMaker.Misc;

public static class SceneManager
{
    private static readonly ExternalPath Path = ExternalPath.Of("scene.json");

    private static Assembly Assembly => Program.Assembly;
    
    private static Error? LoadModels(AssetManager assetManager)
    {
        Models.Registry.Register(Models.ErrorCube,
            new Model(Mesh.Cube(Position.Zero, 1f, TextureQuad.Full), new Model.PerMesh(assetManager.MissingMaterial)));
        
        var calaveraRawModel = new DotGlbReader()
            .Read(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly, "Models", "Calavera", "Calavera.glb"), out var error);
        if (calaveraRawModel == null) return error;
        Models.Registry.Register(Models.Calavera, calaveraRawModel);
        
        Models.Registry.Register(Models.WhiteCube,
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
            Logger.Info(typeof(SceneManager), $"creating default scene because can not load from {Path}: {error}");
            return CreateNewScene();
        }

        var data = JsonSerde.Tight.Deserialize(json, out error);
        if (data == null)
        {
            Logger.Info(typeof(SceneManager), $"creating default scene because can not load from {Path}: {error}");
            return CreateNewScene();
        }

        return Scene.Codec.Decode(data).Map(
            value =>
            {
                Logger.Info(typeof(SceneManager), $"successfully load scene from {Path}");
                return (value, null);
            },
            err =>
            {
                Logger.Error(typeof(SceneManager), "can not load scene: " + err);
                return CreateNewScene();
            });
    }
    
    private static (Scene? Scene, Error? Error) CreateNewScene()
    {
        var scene = new Scene();
        
        // calavera

        scene.Objects.Add(new SObject()
        {
            Model = Models.Calavera,
        });
        
        // lights
        {
            scene.Objects.Add(new SObject()
            {
                Model = Models.WhiteCube,
                Position = (0, 20, -25/2f),
                SpotLight = SpotLight.Ortho(new Vector3(-0.5f, -1, -0.5f), new Color(244/255f, 233/255f, 200/255f, 1f)*1.5f, (30, 30))
            });
                
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
            Logger.Error(typeof(SceneManager), $"can not save scene to {Path}: {error}");
    }
}