using Enjune.File;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Enjune.Graphic.OpenGL.Uniform;

public class TextureArrayUniform
{
    private readonly int _id;

    public TextureArrayUniform(ShaderProgram shader, TextureManager texManager, TextureUnit unit, int unitNumber, string uniformName)
    {
        _id = GL.GenTexture();
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2DArray, _id);
        
        texManager.Compile(out var size, out var texturesDict);
        var textures = texturesDict.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray();

        // allocation
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, size, size, texturesDict.Count);
        
        // params
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        // mipmap generation
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        // loading into
        for (var layer = 0; layer < textures.Length; layer++)
        {
            var texture = textures[layer];
            GL.TexSubImage3D(TextureTarget.Texture2DArray,
                0, // mipmap
                0, 0, layer,
                size, size, 1,
                PixelFormat.Rgba, PixelType.UnsignedByte, texture);
        }
        
        // set uniform
        var location = shader.GetUniformLocation(uniformName);
        GL.Uniform1(location, unitNumber);
    }

    public void Destroy() => GL.DeleteTexture(_id);
    
}