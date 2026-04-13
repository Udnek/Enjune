using Enjune.Graphic;
using OpenTK.Mathematics;

namespace Enjune.World;

public class SObject(Model model, bool isText = false)
{
    public Model Model = model;
    public Position Position = (0, 0, 0);
    public Quaternion Rotation = Quaternion.Identity;

    public bool IsText = isText;
}