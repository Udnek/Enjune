using OpenTK.Mathematics;
using static Minecraft2.Block.BlockFace;

namespace Minecraft2.Block;

public enum BlockFace
{
    Up, Down, North, South, East, West
}

public static class BlockFaceExtensions
{
    extension(BlockFace face)
    {
        public Vector3i Direction()
        {
            return face switch {
                Up => (0, 1, 0),
                Down => (0, -1, 0),
                North => (0, 0, 1),
                South => (0, 0, -1),
                East => (1, 0, 0),
                West => (-1, 0, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
            };
        }
        
        public BlockFace Opposite()
        {
            return face switch {
                Up => Down,
                Down => Up,
                North => South,
                South => North,
                East => West,
                West => East,
                _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
            };
        }
        
        public static BlockFace FromDirection(Vector3i direction)
        {
            return direction switch {
                (0, 1, 0) => Up,
                (0, -1, 0) => Down,
                (0, 0, 1) => North,
                (0, 0, -1) => South,
                (1, 0, 0) => East,
                (-1, 0, 0) => West,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
    

    

}