using OpenTK.Graphics.OpenGL4;

namespace Engine.Graphic.OpenGL.Arrays;

public class Ebo
{
    private readonly int _id;

    public Ebo()
    {
        _id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
    }

    public void BindAndPut(int[] data)
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(int), data, BufferUsageHint.DynamicDraw);
    }

    public void Destroy()
    {
        GL.DeleteBuffer(_id);
    }
}