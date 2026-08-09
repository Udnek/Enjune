using System.Runtime.InteropServices;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Misc;
using Microsoft.VisualBasic;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Data;
using OpenGLApi.Shader;

namespace OpenGLApi.Model;

public class GlModel : GlDisposable, IRenderableModel.IDynamic
{
    [DoNotDisposeViaUtils($"should be disposed in {nameof(OpenGlApi)}")]
    private readonly MaterialShader _shader;
    private readonly int _ssboBinding;
    private readonly bool _final;
    private readonly MatId _whiteMaterialId;

    private Vao? _vao;
    private Vbo<VertexData> _vbo = null!;
    private SsboArray<PerPrimitiveData> _ssbo = null!;
    private Ebo _ebo = null!;
    private int _currentEboLen;

    public IGraphicApi.Primitive CurrentPrimitive { get; private set; }

    public GlModel(MaterialShader shader, int ssboBinding, bool final, MatId whiteMaterialId)
    {
        _shader = shader;
        _ssboBinding = ssboBinding;
        _final = final;
        _whiteMaterialId = whiteMaterialId;
    }
    
    private void Render()
    {
        if (_vao == null || _currentEboLen == 0)
        {
            Logger.Error(this, "can not render: model is empty");
            return;
        }
        _vao.Bind();
        _vbo.Bind();
        _ebo.Bind();
        _ssbo.Bind();
        GL.DrawElements(OpenGlApi.ToGl(CurrentPrimitive), _currentEboLen, DrawElementsType.UnsignedInt, 0);
    }
    
    public void Render(IShader.ICamera.IColor shader) => Render();
    public void Render(IShader.ICamera.IMaterial shader) => Render();
    public void Render(IShader.IShadowMap shader) => Render();

    protected override void DisposeGlData() => Utils.DisposeAllFields(this);

    private void Refit(VertexData[] vboBuf, int[] eboBuf, PerPrimitiveData[] ssboBuf)
    {
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
            const float capacityIncreasement = 1.5f;
            if (_vbo.Capacity < vboBuf.Length) _vbo.Reallocate(CalcCap(_vbo.Capacity, vboBuf.Length));
            if (_ebo.Capacity < eboBuf.Length) _ebo.Reallocate(CalcCap(_ebo.Capacity, eboBuf.Length));
            if (_ssbo.Capacity < ssboBuf.Length) _ssbo.Reallocate(CalcCap(_ssbo.Capacity, ssboBuf.Length));

            static int CalcCap(int current, int model) => (int)Math.Max(current * capacityIncreasement, model);
        }
        _vbo.BindAndPush(vboBuf.ToArray());
        _ebo.BindAndPush(eboBuf.ToArray());
        _currentEboLen = eboBuf.Length;
        _ssbo.BindAndPush(ssboBuf.ToArray());
    }
    
    public void Refit(Enjune.Graphic.Modeling.Model model, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle)
    {
        CurrentPrimitive = primitive;
        
        List<VertexData> vboBuf = new(20);
        List<int> eboBuf = new(20);
        List<PerPrimitiveData> ssboBuf = new(20);

        foreach (var (mesh, perMesh) in model.Meshes)
        {
            var eboOffset = vboBuf.Count;
            foreach (var meshIndex in mesh.Indexes) 
                eboBuf.Add(eboOffset + meshIndex);
            
            // vertices
            for (var i = 0; i < mesh.Vertices.Length; i++)
                vboBuf.Add(new VertexData(mesh.Vertices[i], mesh.PerVertexData[i].TexPos, mesh.PerVertexData[i].Normal));
            
            // materials
            var mat = perMesh.Material?.Id ?? _whiteMaterialId;
            for (var _ = 0; _ < IGraphicApi.PrimitivesAmountFromIndexes(primitive, mesh.Indexes.Length); _++) 
                ssboBuf.Add(new PerPrimitiveData(mat, perMesh.MeshColor));
        }
        
        // TODO optimize by reducing copying
        Refit(vboBuf.ToArray(), eboBuf.ToArray(), ssboBuf.ToArray());
    }
}