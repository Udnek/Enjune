using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public sealed class SpotLight
{
    public Matrix4 View { get; private set; }
    public Matrix4 Projection;
    public Color Color;
    
    public Position Position;
    public Vector3 Direction = -Vector3.UnitY;

    private SpotLight(Matrix4 projection, Color color)
    {
        Projection = projection;
        Color = color;
    }

    public void UpdateView()
    {
        var up = Vector3.UnitY;
        if (1 - Math.Abs(Vector3.Dot(Direction, Vector3.UnitY)) < MathUtils.Epsilon) 
            up = Vector3.UnitX;
        View = Matrix4.LookAt(Position, Position + Direction, up);
    }

    public static SpotLight Perspective(Vector3 direction, Color color, float fovDegree, float near = 0.1f, float far = 100f)
    {
        var light = new SpotLight(
            //Matrix4.CreateOrthographic(5, 2, 0.1f, 100f),
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovDegree), 1, near, far),
            color
        )
        {
            Direction = direction
        };
        light.UpdateView();
        return light;
    }
    
    public static SpotLight Ortho(Vector3 direction, Color color, Vector2i size, float near = 0.1f, float far = 100f)
    {
        var light = new SpotLight(
            Matrix4.CreateOrthographic(size.X, size.Y, near, far),
            color
        )
        {
            Direction = direction
        };
        light.UpdateView();
        return light;
    }
}