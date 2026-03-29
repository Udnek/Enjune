using Enjune.Graphic.InputHandler;
using OpenTK.Mathematics;

namespace Enjune.Graphic;

public interface IGraphicApi
{
    void Init(int width, int height, string title, IUserInputHandler userInputHandler, WindowSizeChangeHandler windowSizeHandler);
    void ViewPort(int x, int y, int width, int height);
    void Title(string title);
    // uniforms
    void Model(Matrix4 model);
    void View(Matrix4 view);
    void Projection(Matrix4 proj);
    // uniforms end
    
    // general pipeline (preferred order)
    bool ShouldStop(); // should stop application
    void ClearVerticesBuffers();
    
    void PutColoredMesh(Mesh mesh, Color color);
    void PutColoredVertex(Position position, Color color, TextureCoord texCoord);
    void PutWhiteVertex(Position position, TextureCoord texCoord);
    
    void ClearScreenBuffers();
    void RenderToScreenBuffer();
    void UpdateScreen();
    void UpdateEvents(); // such as keyboard, mouse, etc
    // general pipeline end
    
    // call #PutVertex before it
    void ProceedBasePipeline()
    {   
        ClearScreenBuffers();
        RenderToScreenBuffer();
        UpdateScreen();
        UpdateEvents();
        ClearVerticesBuffers();
    }
    
    void Destroy();
    void SetClearColor(Color color);

    delegate void WindowSizeChangeHandler(int width, int height);

    enum KeyAction
    {
        Press,
        Release,
        Repeat
    }
}