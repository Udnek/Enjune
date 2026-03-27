using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Arrays;

// ReSharper disable once InconsistentNaming
public class EBO
{
    private readonly int _id;

    public EBO()
    {
        _id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
    }

    public void BindAndPut(int[] data, int sliceLen)
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
        GL.BufferData(BufferTarget.ElementArrayBuffer, sliceLen * sizeof(int), data, BufferUsageHint.DynamicDraw);
    }

    public void Destroy()
    {
        GL.DeleteBuffer(_id);
    }
}