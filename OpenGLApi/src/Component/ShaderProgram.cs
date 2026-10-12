using System.Text;
using Enjune.File;
using Enjune.Graphic.Api.Data;
using Enjune.Misc;
using OpenGLApi.Mesh;

namespace OpenGLApi.Component;

public abstract class ShaderProgram : GlDisposable
{
    private int _program;
    private string _name = null!;
    private int _vertexShader;
    private int _fragmentShader;

    public Error? Init(string name, string vertCode, string fragCode)
    {
        _name = name;
        _program = GL.CreateProgram();
        
        _vertexShader = InitShader(ShaderType.VertexShader, vertCode, out var error);
        if (error != null) return error;
        _fragmentShader = InitShader(ShaderType.FragmentShader, fragCode, out error);
        if (error != null) return error;

        GL.BindFragDataLocation(_program, 0, "fragColor");
        GL.LinkProgram(_program);
        
        // check init
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_program);
            return $"shader program linking failed: {infoLog}";
        }
        
        return null;
    }
    
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

    public static string GenerateShader(ShaderType shaderType, ReadOnlySpan<VaoAttribute> attributes)
    {
        var builder = new StringBuilder();
        builder.Append("#version 430 core");
        foreach (var attr in attributes)
        {
            // layout(location = 0) in vec3 pos;
            builder.Append($"layout(location = {attr.Location}) in {ToGlName(attr.Type)} {attr.InShaderName};");
        }
    }

    private static string ToGlName(ValuePrimitive primitive)
    {
        return primitive switch {
            ValuePrimitive.Int => "int",
            ValuePrimitive.Float => "float",
            ValuePrimitive.Vector3F => "vec3",
            ValuePrimitive.Vector4F => "vec4",
            ValuePrimitive.Matrix3F => "mat3",
            ValuePrimitive.Matrix4F => "mat4",
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
    }
    
    public void Bind() => GL.UseProgram(_program);
    public static void Unbind() => GL.UseProgram(0);
    
    protected override void DisposeGlData()
    {
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteProgram(_program);
    }
}