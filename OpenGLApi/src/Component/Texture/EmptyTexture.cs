using Enjune.File;
using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenGLApi.Component.Texture;

public sealed class EmptyTexture : AbstractTexture
{
    private Vector2i _size;
    
    public EmptyTexture(TextureUnit unit, Vector2i size) : base(TextureTarget.Texture2D, unit)
    {
        _size = size;
        Setup();
    }
    
    public void Resize(Vector2i size)
    {
        _size = size;
        GL.DeleteTexture(Handle);
        Handle = GL.GenTexture();
        BindTo(Unit);
        Setup();
    }

    private void Setup()
    {
        GL.TexImage2D(Target, 0, PixelInternalFormat.Rgb, 
            _size.X, _size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }
    
    public void AttachToFbo(Fbo fbo, FramebufferAttachment attachmentType)
    {
        fbo.Bind();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentType, 
            Target, Handle, 0);
    }

    public override Error? Dump(ExternalPath dir, string namePrefix)
    {
        dir = dir.ThisDirectory();
        Logger.Info(this, $"dumping '{namePrefix}' texture into {dir}");
        byte[] data = new byte[_size.X * _size.Y * 4];
        
        GL.GetTextureImage(Handle, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data.Length, data);
        
        try
        {
            using var image = Image.Load<Rgba32>(data);
            image.Mutate(c => c.Flip(FlipMode.Vertical));
            image.Save(dir.ResolveRaw($"{namePrefix}.png").ToString(), new PngEncoder());
        }
        catch (Exception e)
        {
            return $"can not dump screen texture: {e.Message}";
        }
        
        Logger.Info(this, "done dumping");
        return null;
    }
}