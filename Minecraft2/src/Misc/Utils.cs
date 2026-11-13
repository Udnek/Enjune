using System.Runtime.CompilerServices;

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

    /// <summary>
    /// Pack 4 bytes into uint sequentially in Big Endian like wzyx
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackBytes(byte x, byte y, byte z, byte w = 0) 
        => (uint)x | ((uint)y << 8) | ((uint)z << 16) | ((uint)w << 24);

    /// <summary>
    /// Pack 4 bytes into uint sequentially in Big Endian like wzyx
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackBytes(int x, int y, int z, int w = 0) 
        => (uint)x | ((uint)y << 8) | ((uint)z << 16) | ((uint)w << 24);

    /// <summary>
    /// Converts with clamp floats and packs bytes. See <see cref="PackBytes"/>>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToByteAndPackBytes(Vector4 vec)
    {
        return PackBytes(
            (byte) Math.Clamp(vec.X, 0, 255),
            (byte) Math.Clamp(vec.Y, 0, 255),
            (byte) Math.Clamp(vec.Z, 0, 255),
            (byte) Math.Clamp(vec.W, 0, 255));
    }
    
    /// <summary>
    /// Converts color to uint32 representation. See <see cref="PackBytes"/>>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackColor(Vector4 vec) => ToByteAndPackBytes(vec * 255);

    /// <summary>
    /// Unpacks bytes. See <see cref="PackBytes"/>>
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnpackBytes(uint packed, out byte x, out byte y, out byte z, out byte w)
    {
        x = (byte)((packed >> 0) & 0xFFu);
        y = (byte)((packed >> 8) & 0xFFu);
        z = (byte)((packed >> 16) & 0xFFu);
        w = (byte)((packed >> 24) & 0xFFu);
    }
}