using Enjune.Graphic;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject(Model model)
{
    public Model Model = model;
    public Position Position = (0, 0, 0);
    public Quaternion Rotation = Quaternion.Identity;
}