namespace Minecraft2.Misc;

public static class Utils
{
    /// <summary>
    /// Basically:
    /// for x in [-1, 1]:
    ///     for y in [-1, 1]:
    ///         for z in [-1, 1]:
    ///             action(x, y, z)
    /// </summary>
    public static readonly Vector3i[] FaceNeighbors =
    [
        (-1, 0, 0),
        (1, 0, 0),
        (0, -1, 0),
        (0, 1, 0),
        (0, 0, -1),
        (0, 0, 1)
    ];
}