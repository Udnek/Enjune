namespace Minecraft2.Ecs.System.Misc;

public static class Utils
{
    /// <summary>
    /// Basically:
    /// for x in [-1, 1]:
    ///     for y in [-1, 1]:
    ///         for z in [-1, 1]:
    ///             action(x, y, z)
    /// </summary>
    public static void ForFaceNeighbors(Action<Vector3i> action)
    {
        action((-1, 0, 0));
        action((1, 0, 0));
        action((0, -1, 0));
        action((0, 1, 0));
        action((0, 0, -1));
        action((0, 0, 1));
    }
}