using System.Runtime.CompilerServices;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public abstract class Uniform<T>
{
    private readonly Dictionary<ShaderProgram, int> _shaderToLocation;

    public Uniform(string name, T initialValue, params ShaderProgram[] programs)
    {
        _shaderToLocation = new Dictionary<ShaderProgram, int>(programs.Length);
        foreach (var program in programs)
        {
            program.Bind();
            _shaderToLocation.Add(program, program.GetUniformLocation(name));
            SetValue(program, initialValue);
            program.Unbind();
        }
    }
    
    
    public void SetValue(ShaderProgram shader, T value) => SetValue(_shaderToLocation[shader], value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract void SetValue(int location, T value);
}