using Enjune.File;
using Enjune.Graphic.OpenGL.Uniform;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Enjune.Graphic.OpenGL;

public class ShaderProgram
{
    private readonly int _program;
    private readonly int _vertexShader;
    private readonly int _fragmentShader;
    private readonly TextureManager _textureManager;

    public Matrix4Uniform Model { get; private set; } = null!;
    public Matrix4Uniform View { get; private set; } = null!;
    public Matrix4Uniform Projection { get; private set; } = null!;
    public BoolUniform ColorProvided { get; private set; } = null!;
    private TextureArrayUniform TextureArrayUniform { get; set; } = null!;

    public ShaderProgram(TextureManager textureManager)
    {
        _textureManager = textureManager;
        _program = GL.CreateProgram();

        var fragText = FileManager.LoadText(new ResourcePath("OpenGL", "frag.frag"), out var error)
            ?? throw new Exception($"Fragment shader can not be loaded: {error}");
        var vertText = FileManager.LoadText(new ResourcePath("OpenGL", "vert.vert"), out error)
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
        // use
        GL.UseProgram(_program);
        
        InitUniforms();
    }

    public int GetUniformLocation(string name)
    {
        var location = GL.GetUniformLocation(_program, name);
        if (location == -1) Logger.Error(this, "can not find uniform location " + name);
        return  location;
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

    private void InitUniforms()
    {
        ColorProvided = new BoolUniform(this, UniformColorProvided);
        Model = new Matrix4Uniform(this, UniformModel);
        View = new Matrix4Uniform(this, UniformView);
        Projection = new Matrix4Uniform(this, UniformProjection);
        
        // Set default projection matrix
        var defaultProjection = Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1.0f, 0.1f, 1000.0f);
        Projection.SetValue(defaultProjection);
        
        TextureArrayUniform = new TextureArrayUniform(this, _textureManager, TextureUnit.Texture0, 0,UniformTexture);
    }

    public void Destroy()
    {
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteProgram(_program);
        TextureArrayUniform.Destroy();
    }

    private const string UniformColorProvided = "colorProvided";
    private const string UniformTexture = "textureArray";
    private const string UniformModel = "model";
    private const string UniformView = "view";
    private const string UniformProjection = "projection";
}