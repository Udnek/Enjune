using Enjune.Graphic.Api.Attribute;

namespace OpenGLApi.Generation;

public readonly record struct FieldDesc(
    string InShaderName,
    ShaderValuePrimitive Type
);