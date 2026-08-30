using Enjune.Data.Codec;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public sealed class SpotLight
{
    public static readonly MapCodec<SpotLight> Codec = Codecs.ForEmptyConstructor(() => new SpotLight())
        .ForField("view", i => i.View, (ref i, v) => i.View = v, Codecs.Matrix4)
        .ForField("projection", i => i.Projection, (ref i, v) => i.Projection = v, Codecs.Matrix4)
        .ForField("color", i => i.Color, (ref i, v) => i.Color = v, Codecs.Vector4)
        .ForField("position", i => i.Position, (ref i, v) => i.Position = v, Codecs.Vector3)
        .Build();
    
    // main params
    public Matrix4 View { get; private set; }
    public Matrix4 Projection;
    public Color Color;
    public Position Position;
    
    // utility param
    public Vector3 Direction = -Vector3.UnitY;

    private SpotLight(){}
    
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