namespace OpenGLApi.Component;

public static class DynamicBindings
{
    private static readonly Dictionary<BufferTarget, int> CurrentBinds = [];
    private static int _currentShader = 0;
    
    public static void BindBuffer(BufferTarget target, int handle)
    {
        // to slow?
        // if (CurrentBinds.TryGetValue(target, out var current) && current == handle)
        //     return; // already bind
        // CurrentBinds[target] = handle;
        GL.BindBuffer(target, handle);
    }

    public static void BindShader(int handle)
    {
        if (handle == _currentShader) return;
        _currentShader = handle;
        GL.UseProgram(handle);
    }
}