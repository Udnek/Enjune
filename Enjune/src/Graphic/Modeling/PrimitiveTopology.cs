namespace Enjune.Graphic.Modeling;

public enum PrimitiveTopology
{
    Triangle, // 0-1-2, 3-4-5 ...
    LineStrip, // 0-1-2-3-4..-n
    Line, // 0-1, 2-3, 4-5 ...
    Point // 0, 1, 2, 3 ...
}

public static class PrimitiveTopologyExtensions
{
    extension(PrimitiveTopology primitive)
    {
        public int PrimitivesAmountFromIndexes(int indexes)
        {
            return primitive switch
            {
                PrimitiveTopology.Triangle => indexes / 3,
                PrimitiveTopology.LineStrip => Math.Max(indexes - 1, 0),
                PrimitiveTopology.Line => indexes / 2,
                PrimitiveTopology.Point => indexes,
                _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
            };
        }
    
        public int IndexStride()
        {
            return primitive switch
            {
                PrimitiveTopology.Triangle => 3,
                PrimitiveTopology.LineStrip => 1,
                PrimitiveTopology.Line => 2,
                PrimitiveTopology.Point => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
            };
        }
    }
}