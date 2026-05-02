using Enjune.Graphic.GraphicApi;
using OpenGLApi.Component.Buffer;

namespace OpenGLApi.Shader;

public sealed class ColorShader(Fbo fbo) : CameraShader(fbo), IShader.ICamera.IColor;