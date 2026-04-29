using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject
{
    public IRenderableModel<IShader.I3D.IMaterial>? RMatModel = null;
    public IRenderableModel<IShader.I3D.IColor>? RColorModel = null;
    
    public MaterialModel? MatModel = null;
    public ColorModel? ColorModel = null;
    
    public PointLight? PointLight;


    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;
    public Position Position
    {
        get;
        set
        {
            field = value;
            PointLight?.Position = value;
        }
    }

    // todo optimize by calculating on when changed
    public Matrix4 ModelTransform 
        => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position); // todo wtf why this order works?

    public bool Hidden = false;
}