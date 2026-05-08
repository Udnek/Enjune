using Enjune.Graphic.Api;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;

namespace OpenGLApi.Shader;

public sealed class ColorShader(Fbo fbo, TextureArray textures) : CameraShader(fbo, textures), IShader.ICamera.IColor;