using Enjune.Data;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject
{
    public static readonly Codec<SObject> Codec = Codecs.NewBuilder(() => new SObject())
        .ForField("position", i => i.Position, (ref i, v) => i.Position = v, Codecs.Vector3)
        .ForField("rotation", i => i.Rotation, (ref i, v) => i.Rotation = v, Codecs.Quaternion)
        .ForField("is_realistic", i => i.IsRealistic, (ref i, v) => i.IsRealistic = v, Codecs.Boolean)
        .ForField("hidden", i => i.Hidden, (ref i, v) => i.Hidden = v, Codecs.Boolean)
        .Build();
    
    public IRenderableModel? RenderableModel = null;
    public Model? Model = null;
    
    public SpotLight? PointLight;
    
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;
    public Position Position { get; set; } = Position.Zero;

    public bool IsRealistic = true;
    
    // todo optimize by calculating on when changed
    public Matrix4 ModelTransform => MathUtils.CreateModelTransform(Position, Rotation, Scale);

    public bool Hidden = false;
}