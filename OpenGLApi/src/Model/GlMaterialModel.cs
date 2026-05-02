using Enjune.Graphic.GraphicApi;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Data;
using OpenGLApi.Shader;

namespace OpenGLApi.Model;

public class GlMaterialModel : GlDisposable, IRenderableModel.IMaterial
{
    private readonly Vao _vao;
    private readonly Vbo<MaterialVertexData> _vbo;
    private readonly SsboArray<MatId> _ssbo;
    private readonly Ebo _ebo;
    
    public GlMaterialModel(MaterialShader shader, int ssboMatIdBinding, MaterialModel model)
    {
        List<MaterialVertexData> vboBuf = [];
        List<MatId> ssboBuf = [];
        List<int> eboBuf = [];

        foreach (var (mesh, material) in model.Meshes)
        {
            var eboOffset = vboBuf.Count;
            foreach (var meshIndex in mesh.Indexes) 
                eboBuf.Add(eboOffset + meshIndex);
            
            // vertices
            for (var i = 0; i < mesh.Vertices.Length; i++)
                vboBuf.Add(new MaterialVertexData(mesh.Vertices[i], mesh.PerVertexData[i].texCoord, mesh.PerVertexData[i].normal));
            
            // materials
            for (var _ = 0; _ < mesh.Indexes.Length / 3; _++)
                ssboBuf.Add(material.Id);
        }
        _vao = new Vao();
        _vbo = new Vbo<MaterialVertexData>(vboBuf.Count, vboBuf.ToArray());
        _ebo = new Ebo(eboBuf.Count, eboBuf.ToArray());
        _ssbo = new SsboArray<MatId>(ssboMatIdBinding, ssboBuf.Count, ssboBuf.ToArray());
        
        new VaoAttributes(_vao, _vbo)
            .Add<float>(VertexAttribPointerType.Float, "aPos", 3)
            .Add<float>(VertexAttribPointerType.Float, "aTexPos", 2)
            .Add<float>(VertexAttribPointerType.Float, "aNorm", 3)
            .Compile(shader);
    }

    private void Render()
    {
        _vao.Bind();
        _vbo.Bind();
        _ebo.Bind();
        _ssbo.Bind();
        GL.DrawElements(BeginMode.Triangles, _ebo.Capacity, DrawElementsType.UnsignedInt, 0);
    }

    public void Render(IShader.ICamera.IMaterial shader) => Render();

    public void Render(IShader.IShadowMap shader) => Render();

    protected override void DisposeGlData()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
        _ssbo.Dispose();
    }
}