using Enjune.Graphic;
using Enjune.Graphic.Asset;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject(Model<TextureCoord, CompiledMaterial> model, bool isText = false)
{
    public Model<TextureCoord, CompiledMaterial> Model = model;
    public Position Position = (0, 0, 0);
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4 ModelMatrix =>
        Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateScale(Scale);
    
    public bool IsText = isText;
}