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
        if (VaoVboDevice.Get<TPerVert>().GetIsFailed(out var vaoVboDevice, out var error))
            return new Error($"can not create vaoVboDevice for {Logger.GetTypeName<TPerVert>()}: {error}");
        

        if (BufferDesc.Get<TPerPrim>(StaticBindings.PerPrimitiveSsbo, Std.Std430, "PerPrimitives", "PerPrimitive")
            .GetIsFailed(out var perPrimDesc, out error))
        {
            return new Error($"can not create ssbo desc for {Logger.GetTypeName<TPerPrim>()}: {error}");
        }

        if (StructDesc.Get<TVaryings>(null).GetIsFailed(out var varyings, out error))
            return new Error($"can not create varyings for {Logger.GetTypeName<TVaryings>()}: {error}");

        var vert = ShaderGenerator.CreateVertex(
            userVertexCode,
            _textureSlots.AsSpan(),
            vaoVboDevice.VaoAttributes,
            _ssboDescs.Values.ToArray(),
            _uboDescs.Values.ToArray(),
            varyings
        );

        var frag = ShaderGenerator.CreateFragment(
            userFragmentCode,
            _textureSlots.AsSpan(),
            _ssboDescs.Values.Append(perPrimDesc).ToArray(),
            _uboDescs.Values.ToArray(),
            varyings
        );

        var shader = new ShaderProgram<TPerVert, TPerPrim>();
        _shadersToDispose.Add(shader);
        var shaderError = shader.Init(vert, frag);
        if (shaderError != null)
            return new Error("can not create shader: " + shaderError);

        return ResultOrError.Success<IShader<TPerVert, TPerPrim>>(shader);
    }

    public ResultOrError<IUniform<T>> CreateUniform<T>(string name) where T : unmanaged
    {
        if (BufferDesc.Get<T>(StaticBindings.TakeUbo(name), Std.Std140, name, null)
            .GetIsFailed(out var desc,  out var error))
        {
            return new Error("can not create desc" + error);
        }
        _uboDescs[typeof(T)] = desc;
        var ubo = new Ubo<T>(StaticBindings.TakeUbo(name));
        _bufferToDispose.Add(ubo);
        return ResultOrError.Success<IUniform<T>>(ubo);
    }

    public ResultOrError<IUniformArray<T>> CreateUniformArray<T>(string name, int initialCapacity) where T : unmanaged
    {
        if (BufferDesc.Get<T>(StaticBindings.TakeSsbo(name), Std.Std430, name, null)
            .GetIsFailed(out var desc, out var error))
        {
            return new Error("can not get ssbo desc: " + error);
        }
        
        _ssboDescs[typeof(T)] = desc;
        var ssbo = new Ssbo<T>(StaticBindings.TakeSsbo(name), initialCapacity, false);
        _bufferToDispose.Add(ssbo);
        return ResultOrError.Success<IUniformArray<T>>(ssbo);
    }
}