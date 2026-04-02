namespace Enjune.Graphic.OpenGL.Uniform;

public abstract class Uniform<T>
{
    protected readonly int Location;

    public Uniform(ShaderProgram program, string name)
    {
        Location = program.GetUniformLocation(name);
    }
    
    public abstract void SetValue(T value);
}