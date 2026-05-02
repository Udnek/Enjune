using Enjune.File;

namespace OpenGLApi.Component.Texture;

public abstract class AbstractTexture : GlDisposable
{
    protected int Handle;
    protected readonly TextureTarget Target;
    protected readonly TextureUnit Unit;

    // public static int? PixelFormatToDepth(PixelFormat pixelFormat)
    // {
    //     return pixelFormat switch {}
    // }
    
    
    public AbstractTexture(TextureTarget target, TextureUnit unit)
    {
        Target = target;
        Unit = unit;
        Handle = GL.GenTexture();
        BindTo(Unit);
    }
    
    public abstract Error? Dump(ExternalPath dir, string namePrefix);
    
    public int GetUnitId()
    {
        const int offset = -(int) TextureUnit.Texture0;
        return (int) Unit + offset;
    }
    
    protected void BindTo(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(Target, Handle);
    }

    protected override void DisposeGlData() => GL.DeleteTexture(Handle);
}