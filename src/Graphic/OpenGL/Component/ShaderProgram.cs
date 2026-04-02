using Enjune.File;
using Enjune.Graphic.OpenGL.Component;
using Enjune.Graphic.OpenGL.Uniform;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Enjune.Graphic.OpenGL;

public class ShaderProgram : GLDisposable
{
    private readonly int _program;
    private readonly int _vertexShader;
    private readonly int _fragmentShader;
    private readonly TextureManager _textureManager;

    public ShaderProgram(TextureManager textureManager, ResourcePath fragmentPath, ResourcePath vertexPath)
    {
        _textureManager = textureManager;
        _program = GL.CreateProgram();

        var fragText = FileManager.LoadText(fragmentPath, out var error)
            ?? throw new Exception($"Fragment shader can not be loaded: {error}");
        var vertText = FileManager.LoadText(vertexPath, out error)
            ?? throw new Exception($"Vertex shader can not be loaded: {error}");
        
        _fragmentShader = InitShader(ShaderType.FragmentShader, fragText);
        _vertexShader = InitShader(ShaderType.VertexShader, vertText);

        GL.BindFragDataLocation(_program, 0, "fragColor");
        GL.LinkProgram(_program);

        // check init
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_program);
            throw new Exception($"Shader program linking failed: {infoLog}");
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