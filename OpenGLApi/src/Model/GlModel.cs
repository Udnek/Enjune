using Enjune.Graphic.GraphicApi;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Data;
using OpenGLApi.Shader;

namespace OpenGLApi.Model;

public class GlModel : GlDisposable, IRenderableModel.IDynamic
{
    private readonly MaterialShader _shader;
    private readonly int _ssboBinding;
    private readonly bool _final;
    private readonly MatId _whiteMaterialId;

    private Vao? _vao;
    private Vbo<VertexData> _vbo = null!;
    private SsboArray<PerPrimitiveData> _ssbo = null!;
    private Ebo _ebo = null!;
    private PrimitiveType _glPrimitive;

    public GlModel(MaterialShader shader, int ssboBinding, bool final, MatId whiteMaterialId)
    {
        _shader = shader;
        _ssboBinding = ssboBinding;
        _final = final;
        _whiteMaterialId = whiteMaterialId;
    }

    
    private void Render()
    {
        if (_vao == null)
        {
            Logger.Error(this, "can not render: model is empty");
            return;
        }
        _vao.Bind();
        _vbo.Bind();
        _ebo.Bind();
        _ssbo.Bind();
        GL.DrawElements(_glPrimitive, _ebo.Capacity, DrawElementsType.UnsignedInt, 0);
    }
    

    public void Render(IShader.ICamera.IColor shader) => Render();
    public void Render(IShader.ICamera.IMaterial shader) => Render();
    public void Render(IShader.IShadowMap shader) => Render();

    protected override void DisposeGlData() => Utils.DisposeAllFields(this);

    private void Refit(VertexData[] vboBuf, int[] eboBuf, PerPrimitiveData[] ssboBuf,
        IGraphicApi.Primitive primitive)
    {
        _glPrimitive = OpenGlApi.ToGl(primitive);
        
        if (_vao == null)
        {
            _vao = new Vao();
            _vbo = new Vbo<VertexData>(vboBuf.Length, _final);
            _ebo = new Ebo(eboBuf.Length, _final);
            _ssbo = new SsboArray<PerPrimitiveData>(_ssboBinding, ssboBuf.Length,  _final);
        
            new VaoAttributes(_vao, _vbo)
                .Add<float>(VertexAttribPointerType.Float, "aPos", 3)
                .Add<float>(VertexAttribPointerType.Float, "aTexPos", 2)
                .Add<float>(VertexAttribPointerType.Float, "aNorm", 3)
                .Compile(_shader);
        }
        else
        {
            if (_vbo.Capacity < vboBuf.Length) _vbo.Reallocate(vboBuf.Length);
            if (_ebo.Capacity < eboBuf.Length) _ebo.Reallocate(eboBuf.Length);
            if (_ssbo.Capacity < ssboBuf.Length) _ssbo.Reallocate(ssboBuf.Length);
        }
        _vbo.BindAndPush(vboBuf.ToArray());
        _ebo.BindAndPush(eboBuf.ToArray());
        _ssbo.BindAndPush(ssboBuf.ToArray());
    }
    
    public void Refit(MaterialModel model, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle)
    {
        List<VertexData> vboBuf = [];
        List<int> eboBuf = [];
        List<PerPrimitiveData> ssboBuf = [];

        foreach (var (mesh, material) in model.Meshes)
        {
            var eboOffset = vboBuf.Count;
            foreach (var meshIndex in mesh.Indexes) 
                eboBuf.Add(eboOffset + meshIndex);
            
            // vertices
            for (var i = 0; i < mesh.Vertices.Length; i++)
                vboBuf.Add(new VertexData(mesh.Vertices[i], mesh.PerVertexData[i].texCoord, mesh.PerVertexData[i].normal));
            
            // materials
            for (var _ = 0; _ < IGraphicApi.PrimitivesAmountFromIndexes(primitive, mesh.Indexes.Length); _++) 
                ssboBuf.Add(new PerPrimitiveData(material.Id, Color.One));
        }
        
        Refit(vboBuf.ToArray(), eboBuf.ToArray(), ssboBuf.ToArray(), primitive);
    }

    public void Refit(ColorModel model, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle)
    {
        List<VertexData> vboBuf = [];
        List<int> eboBuf = [];
        List<PerPrimitiveData> ssboBuf = [];

        foreach (var (mesh, color) in model.Meshes)
        {
            var eboOffset = vboBuf.Count;
            foreach (var meshIndex in mesh.Indexes) 
                eboBuf.Add(eboOffset + meshIndex);
            
            // vertices
            for (var i = 0; i < mesh.Vertices.Length; i++)
                vboBuf.Add(new VertexData(mesh.Vertices[i], default, default));
            
            // materials
            int indexId = 0;
            int stride = IGraphicApi.IndexStridePerPrimitive(primitive);
            for (var _ = 0; _ < IGraphicApi.PrimitivesAmountFromIndexes(primitive, mesh.Indexes.Length); _++)
            {
                ssboBuf.Add(new PerPrimitiveData(_whiteMaterialId, mesh.PerVertexData[mesh.Indexes[indexId]] * color));
                indexId += stride;
            }
        }
        
        Refit(vboBuf.ToArray(), eboBuf.ToArray(), ssboBuf.ToArray(), primitive);
    }
}