using OpenTK.Graphics.OpenGL4;

namespace Engine.Graphic.OpenGL.Arrays;

public class Vbo
{
    private readonly int _id;

    public Vbo()
    {
        _id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
    }

    public void BindAndPut(float[] data)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.DynamicDraw);
    }

    public void Destroy()
    {
        GL.DeleteBuffer(_id);
    }
}
