using System.Text;
using Enjune.Attribute;
using Enjune.File;
using Enjune.Graphic.Api;
using Enjune.Graphic.Api.Data;
using Enjune.Misc;
using OpenGLApi.Mesh;

namespace OpenGLApi.Component;

public interface IGlShader : IDisposable;

[LogParams(method: LogParamsAttribute.Method.ToString)]
public sealed class ShaderProgram<TPerVert, TPerPrim> 
    : GlDisposable, IGlShader, IShader<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    private int _program;
    private int _vertexShader;
    private int _fragmentShader;

    public Error? Init(string vertCode, string fragCode)
    {
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
            var lines = source.Split('\n');
            for (var i = 0; i < lines.Length; i++) 
                Logger.Error(this, $"[{i+1}] {lines[i]}");
            error = $"shader compilation failed ({type}): {infoLog}";
            return 0;
        }
        GL.AttachShader(_program, shader);
        error = null;
        return shader;
    }

    public void Bind() => DynamicBindings.BindShader(_program);
    
    protected override void DisposeGlData()
    {
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteProgram(_program);
    }

    public void Render(IRenderableMesh<TPerVert, TPerPrim> mesh)
    {
        Bind();
        // delegating to model
        ((GlMesh<TPerVert, TPerPrim>)mesh).Render();
    }

    public override string ToString() => "ShaderProgram";
}