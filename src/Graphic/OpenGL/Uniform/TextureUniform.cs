using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Enjune.Graphic.OpenGL.Uniform;

public class TextureUniform
{
    private readonly int _id;

    public TextureUniform(ShaderProgram shader, TextureUnit unit, int unitNumber, string uniformName)
    {
        _id = GL.GenTexture();
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        // load
        StbImage.stbi_set_flip_vertically_on_load(1);
        var atlas = FileManager.LoadAtlas();
        GL.TexImage2D(TextureTarget.Texture2D, 0, 
            PixelInternalFormat.Rgba, 
            atlas.Width, atlas.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, atlas.Data);
        
        // generate mipmap
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        // set uniform
        var location = shader.GetUniformLocation(uniformName);
        GL.Uniform1(location, unitNumber);
    }

    public void Destroy() => GL.DeleteTexture(_id);
    
}