using Enjune.File;
using Enjune.Misc;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component;

public class ShaderProgram : GLDisposable
{
    private readonly int _program;
    private readonly int _vertexShader;
    private readonly int _fragmentShader;

    public ShaderProgram(ResourcePath fragmentPath, ResourcePath vertexPath)
    {
        _program = GL.CreateProgram();

        var fragText = fragmentPath.LoadText(out var error)
                       ?? throw new Exception($"Fragment shader can not be loaded: {error}");
        var vertText = vertexPath.LoadText(out error)
                       ?? throw new Exception($"Vertex shader can not be loaded: {error}");
        
        _fragmentShader = InitShader(ShaderType.FragmentShader, fragText);
        _vertexShader = InitShader(ShaderType.VertexShader, vertText);

        GL.BindFragDataLocation(_program, 0, "fragColor");
        GL.LinkProgram(_program);

        // check init
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus != (int) All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_program);
            throw new Exception($"shader program linking failed: {infoLog}");
        }
    }

    public void Bind() => GL.UseProgram(_program);
    public void Unbind() => GL.UseProgram(0);

    public int GetUniformLocation(string name)
    {
        var location = GL.GetUniformLocation(_program, name);
        if (location == -1) Logger.Error(this, "can not find uniform location " + name);
        return location;
    }
    
    // public int GetSsboBinding(string name)
    // {
    //     var index =  GL.GetProgramResourceIndex(_program, ProgramInterface.ShaderStorageBlock, name);
    //     if (index == -1) Logger.Error(this, "can not find ssbo index " + name);
    //     return index;
    // }
    
    public int GetAttributeLocation(string name)
    {
        var location = GL.GetAttribLocation(_program, name);
        if (location == -1) Logger.Error(this, "can not find attribute location " + name);
        return location;
    }
    
    private int InitShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        // check compile
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus);
        if (compileStatus != (int)All.True)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"shader compilation failed ({type}): {infoLog}");
        }
        GL.AttachShader(_program, shader);
        return shader;
    }

    protected override void DisposeGLData()
    {
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteProgram(_program);
    }
}