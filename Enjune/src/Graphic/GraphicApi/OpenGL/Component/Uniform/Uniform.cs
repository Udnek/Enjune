using System.Runtime.CompilerServices;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public abstract class Uniform<T>
{
    private readonly int _location;

    protected Uniform(string name, T initialValue, ShaderProgram program)
    {
        program.Bind();
        _location = program.GetUniformLocation(name);
        SetValue(initialValue);
        ShaderProgram.Unbind();
    }
    
    public void SetValue(T value) => SetValue(_location, value);
    
    protected abstract void SetValue(int location, T value);
}