using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Arrays;

// ReSharper disable once InconsistentNaming
public sealed class VBO : SmartBuffer<float>
{
    private readonly int _id;

    public VBO(int capacity) : base(capacity)
    {
        _id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
    }

    public override void BindAndPush()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        GL.BufferData(BufferTarget.ArrayBuffer, Pointer * sizeof(float), Values, BufferUsageHint.DynamicDraw);
    }

    public void Destroy() => GL.DeleteBuffer(_id);

}
