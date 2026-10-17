using Enjune.Graphic.Api.Data;

namespace OpenGLApi.Generation;

public readonly record struct FieldDesc(
    string InShaderName,
    ShaderValuePrimitive Type
);