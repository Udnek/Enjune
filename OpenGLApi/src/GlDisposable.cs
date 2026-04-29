using Enjune.Misc;

namespace OpenGLApi;

public abstract class GlDisposable : AbstractDisposable
{
    protected abstract void DisposeGlData();

    protected override void DisposeData() => DisposeGlData();
}