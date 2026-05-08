using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject
{
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