using System.Reflection;
using Enjune.Data.Json;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Enjune.Registering;
using OpenTK.Mathematics;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Misc;

public static class ResourceManager
{
    private static readonly ExternalPath WorldPath = ExternalPath.Of("world.json");
    
    public static Error? LoadModels(AssetManager assetManager)
    {
        Models.Registry.Register(Models.ErrorCube,
            new StaticModel(MeshInstance.Of(new MeshInstance.Entry
            {
                Geometry = Mesh.Cube(Position.Zero, 1f, TextureQuad.Full),
                Material = assetManager.MissingMaterial
            })));
        
        var calaveraRawModel = new DotGlbReader()
            .Read(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly, "Models", "Calavera", "Calavera.glb"), out var error);
        if (calaveraRawModel == null) return error;
        Models.Registry.Register(Models.Calavera, calaveraRawModel);
        
        Models.Registry.Register(Models.WhiteCube,
            new StaticModel(MeshInstance.Of(new MeshInstance.Entry
            {
                Geometry = Mesh.Cube(Position.Zero, 0.5f, TextureQuad.Full),
                Material = assetManager.WhiteMaterial
            })));

        return null;
    }
    
    public static ResultOrError<World> LoadOrCreateWorld()
    {
        var json = WorldPath.LoadText(out var error);
        if (json == null)
        {
            Logger.Info(typeof(ResourceManager), $"creating default world because can not load from {WorldPath}: {error}");
            return CreateNewWorld();
        }

        var data = JsonSerde.Tight.Deserialize(json, out error);
        if (data == null)
        {
            Logger.Info(typeof(ResourceManager), $"creating default world because can not load from {WorldPath}: {error}");
            return CreateNewWorld();
        }

        return World.WithoutSystemsCodec.Decode(data);
    }
    
    private static ResultOrError<World> CreateNewWorld()
    {
        var world = new World([]);

        // calavera
        world.AddEntity(new Entity.Assembly()
            .AddComponent(new StaticModelComponent(Models.Calavera))
            .AddComponent(new Transform())
        );

        // lights
        {
            world.AddEntity(new Entity.Assembly()
                .AddComponent(new StaticModelComponent(Models.WhiteCube))
                .AddComponent(new Transform()
                {
                    Position = (0, 20, -25 / 2f),
                    Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, -90)
                })
                .AddComponent(new SpotLightComponent()
                {
                    Projection = Matrix4.CreateOrthographic(30f, 30f, 0.1f, 100f),
                    Color = new Color(244 / 255f, 233 / 255f, 200 / 255f, 1f) * 1.5f
                })
            );
            
            
            world.AddEntity(new Entity.Assembly()
                .AddComponent(new StaticModelComponent(Models.WhiteCube))
                .AddComponent(new Transform()
                {
                    Position = (6, 4, 0),
                    Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, -90)
                })
                .AddComponent(new SpotLightComponent()
                {
                    Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), 1, 0.1f, 100f),
                    Color = new Color(1, 1, 0, 1)
                })
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
                WorldPath.Write(json, out var error);
                return error;
            }).Error;
    }
}