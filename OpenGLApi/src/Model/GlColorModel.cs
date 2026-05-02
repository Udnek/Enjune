using Enjune.Graphic.GraphicApi;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Data;
using OpenGLApi.Shader;

namespace OpenGLApi.Model;

public class GlColorModel : GlDisposable, IRenderableModel.IColor
{
    private readonly Vao _vao;
    private readonly Vbo<ColoredVertexData> _vbo;
    private readonly Ebo _ebo;
    
    public GlColorModel(ColorShader shader, ColorModel model)
    {
        List<ColoredVertexData> vboBuf = [];
        List<int> eboBuf = [];

        foreach (var (mesh, meshColor) in model.Meshes)
        {
            var eboOffset = vboBuf.Count;
            foreach (var meshIndex in mesh.Indexes) 
                eboBuf.Add(eboOffset + meshIndex);
            
            // vertices
            for (var i = 0; i < mesh.Vertices.Length; i++)
                vboBuf.Add(new ColoredVertexData(mesh.Vertices[i], mesh.PerVertexData[i] * meshColor));
            
        }
        _vao = new Vao();
        _vbo = new Vbo<ColoredVertexData>(vboBuf.Count, vboBuf.ToArray());
        _ebo = new Ebo(eboBuf.Count, eboBuf.ToArray());
        
        new VaoAttributes(_vao, _vbo)
            .Add<float>(VertexAttribPointerType.Float, "aPos", 3)
            .Add<float>(VertexAttribPointerType.Float, "aColor", 4)
            .Compile(shader);
    }
    
    public void Render(IShader.ICamera.IColor shader, IGraphicApi.Primitive primitive)
    {
        _vao.Bind();
        _vbo.Bind();
        _ebo.Bind();
        GL.DrawElements(OpenGlApi.ToGl(primitive), _ebo.Capacity, DrawElementsType.UnsignedInt, 0);
    }

    protected override void DisposeGlData()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
    }
}