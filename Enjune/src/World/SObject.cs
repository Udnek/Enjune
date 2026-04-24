using Enjune.Graphic;
using Enjune.Graphic.Asset;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject(Model<(TextureCoord, Vector3), CompiledMaterial>? matModel = null, bool isText = false)
{
    public Model<(TextureCoord, Vector3), CompiledMaterial>? MatModel = matModel;
    public Model<Color, Color>? ColorModel = null;
    public Position Position = (0, 0, 0);
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4 ModelMatrix =>
        Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateScale(Scale);
    
    public bool IsText = isText;

    public bool Hidden = false;
}