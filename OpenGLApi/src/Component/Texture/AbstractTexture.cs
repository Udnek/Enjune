using Enjune.File;

namespace OpenGLApi.Component.Texture;

public abstract class AbstractTexture : GlDisposable
{
    public int Handle { get; protected set; }
    protected readonly TextureTarget Target;
    
    public AbstractTexture(TextureTarget target)
    {
        Target = target;
        GenTexture();
    }

    protected void GenTexture()
    {
        GL.CreateTextures(Target, 1, out int textures);
        Handle = textures;
    }
    
    public void BindTo(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(Target, Handle);
    }
    
    protected void SetParams()
    {
        GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    } 

    protected override void DisposeGlData() => GL.DeleteTexture(Handle);
}