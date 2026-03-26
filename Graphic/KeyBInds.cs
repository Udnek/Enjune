using OpenTK.Graphics.ES11;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Graphic;

public static class KeyBinds
{
    private static readonly Dictionary<GLKeyId, KeyBind> Binds = new();

    public static readonly KeyBind Forward = Bind(new KeyBind("forward", GLKeyId.W, true));
    public static readonly KeyBind Backward = Bind(new KeyBind("backward", GLKeyId.S, true));
    public static readonly KeyBind Leftward = Bind(new KeyBind("leftward", GLKeyId.A, true));
    public static readonly KeyBind Rightward = Bind(new KeyBind("rightward", GLKeyId.D, true));

    public static readonly KeyBind Upward = Bind(new KeyBind("upward", GLKeyId.Space, true));
    public static readonly KeyBind Downward = Bind(new KeyBind("downward", GLKeyId.LeftShift, true));

    public static readonly KeyBind LookUp = Bind(new KeyBind("look_up", GLKeyId.Up, true));
    public static readonly KeyBind LookDown = Bind(new KeyBind("look_down", GLKeyId.Down, true));
    public static readonly KeyBind LookLeft = Bind(new KeyBind("look_left", GLKeyId.Left, true));
    public static readonly KeyBind LookRight = Bind(new KeyBind("look_right", GLKeyId.Right, true));

    public static readonly KeyBind DebugMenu = Bind(new KeyBind("debug", GLKeyId.F3));

    public static KeyBind Bind(KeyBind keyBind)
    {
        if (Binds.TryGetValue(keyBind.GlfwKeyId, out var existed))
        {
            Console.WriteLine($"Rebinded key from {existed} to {keyBind}");
        }
        Binds[keyBind.GlfwKeyId] = keyBind;
        return keyBind;
    }

    public class KeyBind(string name, GLKeyId glfwKeyId, bool continuousPress = false)
    {
        public readonly string Name = name;
        public readonly GLKeyId GlfwKeyId = glfwKeyId;
        public readonly bool ContinuousPress = continuousPress;

        public override string ToString()
        {
            return $"Bind(name='{Name}', glfwKeyId={GlfwKeyId})";
        }
    }
}