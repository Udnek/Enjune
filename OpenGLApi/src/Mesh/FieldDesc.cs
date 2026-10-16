using Enjune.Graphic.Api.Data;

namespace OpenGLApi.Mesh;

public readonly record struct FieldDesc(
    string InShaderName,
    ShaderValuePrimitive Type
);