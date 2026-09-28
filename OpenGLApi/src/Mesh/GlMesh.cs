using Enjune.Attribute;
using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Data;
using OpenGLApi.Shader;

namespace OpenGLApi.Mesh;

public class GlMesh : GlDisposable, IRenderableMesh.IDynamic
{
    [DoNotDisposeViaUtils($"should be disposed in {nameof(OpenGlApi)}")]
    private readonly MaterialShader _shader;
    private readonly int _ssboBinding;
    private readonly bool _final;
    private readonly MatId _whiteMaterialId; // fallback, if it is unspecified in model

    private Vao? _vao;
    private Vbo<VertexData> _vbo = null!;
    private SsboArray<PerPrimitiveData> _ssbo = null!;
    private Ebo _ebo = null!;
    private int _currentEboLen;

    public PrimitiveType CurrentTopology { get; private set; }

    public GlMesh(MaterialShader shader, int ssboBinding, bool final, MatId whiteMaterialId)
    {
        _shader = shader;
        _ssboBinding = ssboBinding;
        _final = final;
        _whiteMaterialId = whiteMaterialId;
    }
    
    public void Render()
    {
        if (_vao == null || _currentEboLen == 0)
            return;
        
        _vao.Bind();
        _vbo.Bind();
        _ebo.Bind();
        _ssbo.Bind();
        GL.DrawElements(CurrentTopology, _currentEboLen, DrawElementsType.UnsignedInt, 0);
    }

    protected override void DisposeGlData() => Utils.DisposeAllFields(this);

    private void Refit(ReadOnlySpan<VertexData> vboBuf, ReadOnlySpan<int> eboBuf, ReadOnlySpan<PerPrimitiveData> ssboBuf)
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
        _vbo.BindAndPush(vboBuf);
        _ebo.BindAndPush(eboBuf);
        _currentEboLen = eboBuf.Length;
        _ssbo.BindAndPush(ssboBuf);
    }
    
    public void Refit(MeshInstance mesh)
    {
        CurrentTopology = OpenGlApi.ToGl(mesh.CommonTopology);

        // counting
        int vertices = 0;
        int indexes = 0;
        foreach (var entry in mesh.Entries)
        {
            vertices += entry.Geometry.Vertices.Length;
            indexes += entry.Geometry.Indexes.Length;
        }
        
        // initializing
        var primitives = mesh.CommonTopology.PrimitivesAmountFromIndexes(indexes);
        List<VertexData> vboBuf = new(vertices);
        List<int> eboBuf = new(indexes);
        List<PerPrimitiveData> ssboBuf = new(primitives);
        
        // filling
        int indexOffset = 0;
        foreach (var entry in mesh.Entries)
        {
            var geometry = entry.Geometry;
            // ssbo
            var perPrimitive = new PerPrimitiveData(entry.Material?.Id ?? _whiteMaterialId, entry.Color);
            for (int i = 0; i < mesh.CommonTopology.PrimitivesAmountFromIndexes(geometry.Indexes.Length); i++)
            {
                ssboBuf.Add(perPrimitive);   
            }
            // vbo
            for (int i = 0; i < geometry.Vertices.Length; i++)
            {
                var (texPos, normal) = geometry.PerVertexData[i];
                vboBuf.Add(new VertexData(geometry.Vertices[i], texPos, normal));
            }
            // ebo
            foreach (var index in geometry.Indexes) 
                eboBuf.Add(indexOffset + index);
            
            // increasing index offset for further entries
            indexOffset += geometry.Vertices.Length;
        }

        Refit(vboBuf.AsSpan(), eboBuf.AsSpan(), ssboBuf.AsSpan());
    }
}