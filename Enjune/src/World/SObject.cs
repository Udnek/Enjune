using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject(Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>? matModel = null, bool isText = false)
{
    public Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>? MatModel = matModel;
    public Model<Color, Color>? ColorModel = null;
    public Position Position = (0, 0, 0);
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;
    public PointLight? PointLight;

    public Matrix4 ModelMatrix =>
        Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position); // todo wtf why this order works?
    
    public bool IsText = isText;

    public bool Hidden = false;
}