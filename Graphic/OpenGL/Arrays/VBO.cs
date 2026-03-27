using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Arrays;

// ReSharper disable once InconsistentNaming
public class VBO
{
    private readonly int _id;

    public VBO()
    {
        _id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
    }

    public void BindAndPut(float[] data, int sliceLen)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        GL.BufferData(BufferTarget.ArrayBuffer, sliceLen * sizeof(float), data, BufferUsageHint.StaticDraw);
    }

    public void Destroy()
    {
        GL.DeleteBuffer(_id);
    }
}
