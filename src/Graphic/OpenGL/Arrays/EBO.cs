using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Arrays;

// ReSharper disable once InconsistentNaming
public sealed class EBO : SmartBuffer<int>
{
    private readonly int _id;

    public EBO(int capacity) : base(capacity)
    {
        _id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
    }

    public override void BindAndPush()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
        GL.BufferData(BufferTarget.ElementArrayBuffer, Count * sizeof(int), Values, BufferUsageHint.DynamicDraw);
    }

    public void Destroy() => GL.DeleteBuffer(_id);
}