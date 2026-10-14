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

    public static string GenerateShader(
        ShaderType shaderType,
        ReadOnlySpan<VaoAttribute> attributes,
        ReadOnlySpan<BufferDesc> ssbos,
        ReadOnlySpan<BufferDesc> ubos,
        BufferDesc varyings)
    {
        var builder = new StringBuilder();
        // header
        builder.Append("#version 430 core");

        if (shaderType == ShaderType.VertexShader)
        {
            // attributes
            foreach (var attr in attributes)
            {
                // layout(location = 0) in vec3 pos;
                builder.Append($"layout(location = {attr.Location}) in {ToGlName(attr.Type)} {attr.InShaderName};");
            }
            // varyings out
            for (var i = 0; i < varyings.StructFields.Count; i++)
            {
                var varying = varyings.StructFields[i];
                // layout(location = 0) out vec4 color;
                builder.Append($"layout(location = {i}) out {ToGlName(varying.Type)} {varying.InShaderName};");
            }
        } else if (shaderType == ShaderType.FragmentShader)
        {
            // varyings in
            for (var i = 0; i < varyings.StructFields.Count; i++)
            {
                var varying = varyings.StructFields[i];
                // layout(location = 0) in vec4 color;
                builder.Append($"layout(location = {i}) in {ToGlName(varying.Type)} {varying.InShaderName};");
            }
        }
        
        // ssbos
        const string structPostfix = "_struct";
        const string bufferPostfix = "_buff";
        foreach (var ssbo in ssbos)
        {
            // struct PerPrim_struct {
            builder.Append($"struct {ssbo.InShaderName}{structPostfix} {{");
            foreach (var field in ssbo.StructFields)
            {
                //      int matId;
                builder.Append($"   {ToGlName(field.Type)} {field.InShaderName};");
            }
            // };
            builder.Append("};");
            
            // layout(std430, binding = 0) readonly buffer PerPrim_buff {
            builder.Append($"layout(std430, binding = {ssbo.Binding}) readonly buffer {ssbo.InShaderName}{bufferPostfix} {{");
            //     PerPrim_struct PerPrim[];
            builder.Append($"    {ssbo.InShaderName}{structPostfix} {ssbo.InShaderName}[];");
            // };
            builder.Append("};");
        }
        
        // ubo
        foreach (var ubo in ubos)
        {
            // layout(std140, binding = 0) uniform CameraUniforms {
            builder.Append($"layout(std140, binding = {ubo.Binding}) uniform {ubo.InShaderName} {{");
            foreach (var field in ubo.StructFields)
            {
                //     mat4 View;
                builder.Append($"    {ToGlName(field.Type)} {field.InShaderName};");
            }
            // };
            builder.Append("};");
        }
        
        
        
        return builder.
    }

    private static string ToGlName(ShaderValuePrimitive primitive)
    {
        return primitive switch {
            ShaderValuePrimitive.Int => "int",
            ShaderValuePrimitive.Float => "float",
            ShaderValuePrimitive.Vector2F => "vec2",
            ShaderValuePrimitive.Vector3F => "vec3",
            ShaderValuePrimitive.Vector4F => "vec4",
            ShaderValuePrimitive.Matrix3F => "mat3",
            ShaderValuePrimitive.Matrix4F => "mat4",
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