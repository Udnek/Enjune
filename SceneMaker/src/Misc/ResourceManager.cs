using System.Reflection;
using Enjune.Data.Json;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Enjune.Registering;
using Enjune.World;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Misc;

public static class ResourceManager
{
    private static readonly ExternalPath Path = ExternalPath.Of("world.json");

    private static Assembly Assembly => Program.Assembly;
    
    public static Error? LoadModels(AssetManager assetManager)
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
    
    public static ResultOrError<World> LoadWorld()
    {
        var json = Path.LoadText(out var error);
        if (json == null)
        {
            Logger.Info(typeof(ResourceManager), $"creating default world because can not load from {Path}: {error}");
            return CreateNewWorld();
        }

        var data = JsonSerde.Tight.Deserialize(json, out error);
        if (data == null)
        {
            Logger.Info(typeof(ResourceManager), $"creating default world because can not load from {Path}: {error}");
            return CreateNewWorld();
        }

        return World.WithoutSystemsCodec.Decode(data);
    }
    
    private static ResultOrError<World> CreateNewWorld()
    {
        var world = new World([], [typeof(Transform), typeof(ModelComponent), typeof(SpotLightComponent)]);

        // calavera
        world.AddEntity(new Entity.Assembly()
            .AddComponent(new ModelComponent(Models.Calavera))
            .AddComponent(new Transform())
        );

        // lights
        {
            world.AddEntity(new Entity.Assembly()
                .AddComponent(new ModelComponent(Models.WhiteCube))
                .AddComponent(new Transform()
                {
                    Position = (0, 20, -25 / 2f)
                })
                .AddComponent(new SpotLightComponent(SpotLight.Ortho(
                    new Vector3(-0.5f, -1, -0.5f),
                    new Color(244 / 255f, 233 / 255f, 200 / 255f, 1f) * 1.5f, 
                    (30, 30))))
            );
            
            
            world.AddEntity(new Entity.Assembly()
                .AddComponent(new ModelComponent(Models.WhiteCube))
                .AddComponent(new Transform()
                {
                    Position = (6, 4, 0)
                })
                .AddComponent(new SpotLightComponent(SpotLight.Perspective(-Vector3.UnitY, new Color(1, 1, 0, 1), 45f)))
            );
        }
        return ResultOrError.Success(world);
    }

    public static Error? Save(World world)
    {
        return World.WithoutSystemsCodec.Encode(world)
            .AndThen(data => JsonSerde.Indent4.Serialize(data))
            .AndThen(json =>
            {
                Path.Write(json, out var error);
                return error;
            }).Error;
    }
}