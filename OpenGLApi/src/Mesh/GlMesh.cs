using Enjune.Attribute;
using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Data;
using OpenGLApi.Shader;

namespace OpenGLApi.Mesh;

public interface IGlMesh : IDisposable;

public class GlMesh<TPerVert, TPerPrim> : GlDisposable, IRenderableMesh<TPerVert, TPerPrim>.IDynamic, IGlMesh where TPerVert : unmanaged where TPerPrim : unmanaged
{
    private readonly bool _final;
    private readonly MatId _whiteMaterialId; // fallback, if it is unspecified in model
    private readonly VaoVboDevice<TPerVert> _vaoVboDevice;
    
    private Vao? _vao;
    private Vbo<TPerVert> _vbo = null!;
    private Ssbo<TPerPrim> _ssbo = null!;
    private Ebo _ebo = null!;

    public PrimitiveType CurrentTopology { get; private set; }

    public GlMesh(VaoVboDevice<TPerVert> vaoVboDevice, bool final, MatId whiteMaterialId)
    {
        _vaoVboDevice = vaoVboDevice;
        _final = final;
        _whiteMaterialId = whiteMaterialId;
    }
    
    public void Render()
    {
        if (_vao == null || _ebo.Count == 0)
            return;
        
        _vao.Bind();
        _vbo.Bind();
        _ebo.Bind();
        _ssbo.Bind();
        GL.DrawElements(CurrentTopology, _ebo.Count, DrawElementsType.UnsignedInt, 0);
    }

    protected override void DisposeGlData()
    {
        if (_vao is null) return; // nothing to dispose
        Utils.DisposeAllFields(this);
    }

    private void Refit(ReadOnlySpan<TPerVert> vboBuf, ReadOnlySpan<int> eboBuf, ReadOnlySpan<TPerPrim> ssboBuf)
    {
        if (eboBuf.IsEmpty) // model is empty
            return;
        
        if (_vao == null) // nothing initialized yet
        {
            _vaoVboDevice.Create(_final, vboBuf.Length,
                out _vbo, 
                out _vao);
            _ebo = new Ebo(eboBuf.Length, _final);
            _ssbo = new Ssbo<TPerPrim>(Bindings.PerPrimitiveSsbo, ssboBuf.Length,  _final);
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
        var totalPrimitives = mesh.CommonTopology.PrimitivesAmountFromIndexes(indexes);
        List<VertexData> vboBuf = new(vertices);
        List<int> eboBuf = new(indexes);
        List<PerPrimitiveData> ssboBuf = new(totalPrimitives);
        
        // filling
        int indexOffset = 0;
        foreach (var entry in mesh.Entries)
        {
            var geometry = entry.Geometry;
            // ssbo
            var perPrimitive = new PerPrimitiveData(entry.Material?.Id ?? _whiteMaterialId, entry.Color);
            var entryPrimitives = mesh.CommonTopology.PrimitivesAmountFromIndexes(geometry.Indexes.Length);
            for (int i = 0; i < entryPrimitives; i++)
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