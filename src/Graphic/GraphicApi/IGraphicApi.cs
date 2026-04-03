using Enjune.Graphic.InputHandler;
using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi;

public interface IGraphicApi : IDisposable
{
    void Init(TextureManager textureManager, int width, int height, string title, IUserInputHandler userInputHandler, WindowSizeChangeHandler windowSizeHandler);
    void ViewPort(int x, int y, int width, int height);
    void Title(string title);
    // uniforms
    void Model(Matrix4 model);
    void View(Matrix4 view);
    void Projection(Matrix4 proj);
    // uniforms end
    
    public void SetMaterials(MaterialBuffer materials);
    
    // general pipeline (preferred order)
    bool ShouldStop(); // should stop application
    void ClearScreenBuffers();
    void RenderToScreenBuffer(VertexBuffer buffer);
    void UpdateScreen();
    void UpdateEvents(); // such as keyboard, mouse, etc
    // general pipeline end
    
    void SetClearColor(Color color);

    void DumpTextures();
    
    delegate void WindowSizeChangeHandler(int width, int height);

    enum KeyAction
    {
        Press,
        Release,
        Repeat
    }
}