using Enjune.Data.Codec;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public sealed class SpotLight(Matrix4 view, Matrix4 projection, Color color, Position position)
{
    public static readonly MapCodec<SpotLight> Codec = Codecs.ForEmptyConstructor(() => new SpotLight())
        .ForField("view", i => i.View, (ref i, v) => i.View = v, Codecs.Matrix4)
        .ForField("projection", i => i.Projection, (ref i, v) => i.Projection = v, Codecs.Matrix4)
        .ForField("color", i => i.Color, (ref i, v) => i.Color = v, Codecs.Vector4)
        .ForField("position", i => i.Position, (ref i, v) => i.Position = v, Codecs.Vector3)
        .Build();


    public Matrix4 View = view;
    public Matrix4 Projection = projection;
    public Color Color = color;
    public Position Position = position;

    public SpotLight() : this(Matrix4.Identity, Matrix4.Identity, Color.One, Vector3.Zero)
    {
    }
    
    public static SpotLight Perspective(Position pos, Vector3 direction, Color color, float fovDegree, float near = 0.1f, float far = 100f)
    {
        return new SpotLight(
            MathUtils.CreateView(pos, direction),
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovDegree), 1, near, far),
            color,
            pos
        );
    }
    
    public static SpotLight Ortho(Position pos, Vector3 direction, Color color, Vector2i size, float near = 0.1f, float far = 100f)
    {
        return new SpotLight(
            MathUtils.CreateView(pos, direction),
            Matrix4.CreateOrthographic(size.X, size.Y, near, far),
            color,
            pos
        );
    }
}