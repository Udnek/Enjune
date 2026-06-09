using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.Input;

public record struct UniKey
{
    private GlfwKey? _glfwKey;
    private MouseButton? _glfwButton;
    
    public static UniKey Of(GlfwKey glfwKey) => new(glfwKey, null);

    public static UniKey Of(MouseButton? mouseButton) => new(null, mouseButton);
    
    private UniKey(GlfwKey? glfwKey, MouseButton? glfwButton)
    {
        _glfwKey = glfwKey;
        _glfwButton = glfwButton;
    }
}