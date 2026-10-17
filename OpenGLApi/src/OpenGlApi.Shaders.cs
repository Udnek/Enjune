using Enjune.Graphic.Api;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Generation;
using OpenGLApi.Mesh;

namespace OpenGLApi;

public sealed partial class OpenGlApi
{
    public ResultOrError<IShader<TPerVert, TPerPrim>> CreateShader<TPerVert, TPerPrim, TVaryings>(string userVertexCode,
        string userFragmentCode) where TPerVert : unmanaged where TPerPrim : unmanaged where TVaryings : unmanaged
    {
        var vaoVboDeviceOrErr = VaoVboDevice.Get<TPerVert>();
        if (vaoVboDeviceOrErr.Error is not null)
            return vaoVboDeviceOrErr.Error.Value;

        var ssboDescOrErr =
            BufferDesc.Get<TPerPrim>(StaticBindings.PerPrimitiveSsbo, Std.Std430, "PerPrimitives", "PerPrimitive");
        if (ssboDescOrErr.Error is not null)
            return ssboDescOrErr.Error.Value;

        _ssboDescs[typeof(TPerPrim)] = ssboDescOrErr.GetOrThrow();

        var varyingsOrErr = StructDesc.Get<TVaryings>(null);
        if (varyingsOrErr.Error is not null)
            return varyingsOrErr.Error.Value;

        var vert = ShaderGenerator.CreateVertex(
            userVertexCode,
            _textureSlots.AsSpan(),
            vaoVboDeviceOrErr.GetOrThrow().VaoAttributes,
            _ssboDescs.Values.ToArray(),
            _uboDescs.Values.ToArray(),
            varyingsOrErr.GetOrThrow()
        );

        var frag = ShaderGenerator.CreateFragment(
            userFragmentCode,
            _textureSlots.AsSpan(),
            _ssboDescs.Values.ToArray(),
            _uboDescs.Values.ToArray(),
            varyingsOrErr.GetOrThrow()
        );

        var shader = new ShaderProgram<TPerVert, TPerPrim>();
        _shadersToDispose.Add(shader);
        var shaderError = shader.Init(vert, frag);
        if (shaderError != null)
            return shaderError.Value;

        return ResultOrError.Success<IShader<TPerVert, TPerPrim>>(shader);
    }

    public ResultOrError<IUniform<T>> CreateUniform<T>(string name) where T : unmanaged
    {
        var descOrErr = BufferDesc.Get<T>(StaticBindings.TakeUbo(name), Std.Std140, name, null);
        if (descOrErr.Error != null)
            return descOrErr.Error.Value;
        var desc = descOrErr.GetOrThrow();
        _uboDescs[typeof(T)] = desc;
        var ubo = new Ubo<T>(StaticBindings.TakeUbo(name));
        _bufferToDispose.Add(ubo);
        return ResultOrError.Success<IUniform<T>>(ubo);
    }

    public ResultOrError<IUniformArray<T>> CreateUniformArray<T>(string name, int initialCapacity) where T : unmanaged
    {
        var descOrErr = BufferDesc.Get<T>(StaticBindings.TakeSsbo(name), Std.Std430, name, null);
        if (descOrErr.Error != null)
            return descOrErr.Error.Value;
        var desc = descOrErr.GetOrThrow();
        _ssboDescs[typeof(T)] = desc;
        var ssbo = new Ssbo<T>(StaticBindings.TakeSsbo(name), initialCapacity, false);
        _bufferToDispose.Add(ssbo);
        return ResultOrError.Success<IUniformArray<T>>(ssbo);
    }
}