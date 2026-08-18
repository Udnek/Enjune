using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Enjune.Registering;
using OpenTK.Mathematics;

namespace Enjune.World;

[Obsolete]
public class SObject
{
    public static readonly MapCodec<SObject> Codec = Codecs
        .ForEmptyConstructor(() => new SObject())
        .ForField("position", i => i.Position, (ref i, v) => i.Position = v, Codecs.Vector3)
        .ForField("rotation", i => i.Rotation, (ref i, v) => i.Rotation = v, Codecs.Quaternion)
        .ForField("scale", i => i.Scale, (ref i, v) => i.Scale = v, Codecs.Vector3)
        
        //.ForField("model", i => i.Model!, (ref i, v) => i.Model = v, RegistrableModel.Codec)
        .ForField("is_realistic", i => i.IsRealistic, (ref i, v) => i.IsRealistic = v, Codecs.Boolean)
        .ForField("hidden", i => i.Hidden, (ref i, v) => i.Hidden = v, Codecs.Boolean)
        
        .ForField("spot_light", i => i.SpotLight, (ref i, v) => i.SpotLight = v, Codecs.NullableOf(SpotLight.Codec))
        .Build();

    public bool ToBeSerialized = true;
    
    public IRenderableModel? RenderableModel;
    public RegistryReference<Model>? Model;
    
    public SpotLight? SpotLight;
    
    public Position Position { get; set; } = Position.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;

    public bool IsRealistic = true;
    
    // todo optimize by calculating on when changed
    public Matrix4 ModelTransform => MathUtils.CreateModelTransform(Position, Rotation, Scale);

    public bool Hidden = false;
}