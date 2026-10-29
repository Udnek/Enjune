using Enjune.Ecs.Component;
using Enjune.Registering;

using OpenTK.Mathematics;
using System.Reflection;

namespace PhysicsLibrary.General.Component;

public interface ITransform 
{
    public Vector3 Position { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Quaternion Rotation { get; set; }
}
