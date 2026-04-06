using Enjune.Graphic;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject(Model model, Position position, Quaternion rotation)
{
    public Model Model = model;
    public Position Position = position;
    public Quaternion Rotation = rotation;
}