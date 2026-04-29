using Enjune.File;
using Enjune.Misc;

namespace OpenGLApi.Component;

public abstract class ShaderProgram : GlDisposable
{
    private int _program;
    private string _vertexShaderName = null!;
    private int _vertexShader;
    private int _fragmentShader;

    public Error? Init(ResourcePath fragmentPath, ResourcePath vertexPath)
    {
        _program = GL.CreateProgram();

        var fragText = fragmentPath.LoadText(out var error);
        if (fragText == null) return $"fragment shader can not be loaded: {error}";
        var vertText = vertexPath.LoadText(out error);
        if (vertText == null) return $"vertex shader can not be loaded: {error}";

        _fragmentShader = InitShader(ShaderType.FragmentShader, fragText, out error);
        if (error != null) return error;
        _vertexShader = InitShader(ShaderType.VertexShader, vertText, out error);
        if (error != null) return error;

        GL.BindFragDataLocation(_program, 0, "fragColor");
        GL.LinkProgram(_program);

        _vertexShaderName = vertexPath.ToString();
        
        // check init
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_program);
            return $"shader program linking failed: {infoLog}";
        }
        // uniforms
        InitUniforms();
        return null;
    }
    
    protected abstract void InitUniforms();
    
    public void Bind() => GL.UseProgram(_program);
    public static void Unbind() => GL.UseProgram(0);
    
    private int InitShader(ShaderType type, string source, out Error? error)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        // check compile
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus);
        if (compileStatus != (int)All.True)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            error = $"shader compilation failed ({type}): {infoLog}";
            return 0;
        }
        GL.AttachShader(_program, shader);
        error = null;
        return shader;
    }
    
    public int GetUniformLocation(string name)
    {
        var location = GL.GetUniformLocation(_program, name);
        if (location == -1) Logger.Error(this, "can not find uniform location " + name);
        return location;
    }
    
    public int GetAttributeLocation(string name)
    {
        var location = GL.GetAttribLocation(_program, name);
        if (location == -1) Logger.Error(this, $"can not find attribute location {name} in shader {_vertexShaderName}");
        return location;
    }

    protected override void DisposeGlData()
    {
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteProgram(_program);
    }
}