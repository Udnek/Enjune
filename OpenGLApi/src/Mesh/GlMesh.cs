using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Generation;

namespace OpenGLApi.Mesh;

public interface IGlMesh : IDisposable;

public class GlMesh<TPerVert, TPerPrim> : GlDisposable, IRenderableMesh<TPerVert, TPerPrim>.IDynamic, IGlMesh where TPerVert : unmanaged where TPerPrim : unmanaged
{
    private readonly bool _fixedSize;
    private readonly VaoVboDevice<TPerVert> _vaoVboDevice;
    
    private Vao? _vao;
    private Vbo<TPerVert> _vbo = null!;
    private Ssbo<TPerPrim> _ssbo = null!;
    private Ebo _ebo = null!;

    public PrimitiveType CurrentTopology { get; private set; }

    public GlMesh(VaoVboDevice<TPerVert> vaoVboDevice, bool fixedSize)
    {
        _vaoVboDevice = vaoVboDevice;
        _fixedSize = fixedSize;
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

    private void Refit(ReadOnlySpan<TPerVert> vboBuf, ReadOnlySpan<int> eboBuf, ReadOnlySpan<TPerPrim> ssboBuf)
    {
        if (eboBuf.IsEmpty) // model is empty
            return;
        
        if (_vao == null) // nothing initialized yet
        {
            _vaoVboDevice.Create(_fixedSize, vboBuf.Length,
                out _vbo, 
                out _vao);
            _ebo = new Ebo(eboBuf.Length, _fixedSize);
            _ssbo = new Ssbo<TPerPrim>(StaticBindings.PerPrimitiveSsbo, ssboBuf.Length, _fixedSize);
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
    
    public void Refit(Mesh<TPerVert, TPerPrim> mesh)
    {
        CurrentTopology = OpenGlApi.ToGl(mesh.Topology);
        Refit(mesh.Vertices.AsSpan(), mesh.Indexes.AsSpan(), mesh.PerPrimitives.AsSpan());
    }
    
    protected override void DisposeGlData()
    {
        if (_vao is null) return; // nothing to dispose
        Utils.DisposeAllFields(this);
    }
}