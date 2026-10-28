using System.Diagnostics;
using System.Numerics;

namespace Enjune.Misc;

using uint64 = UInt64;

public class PaletteMappedArray<T> where T : IEquatable<T>
{
    private uint64[] _chunks;
    private readonly List<T> _idToElement;
    private readonly Dictionary<T, int> _elementToId;
    
    private const int ChunkBitSize = sizeof(uint64) * 8; // 64 basically
    private int _paletteSize;
    
    public PaletteMappedArray(int initialCapacity, int initialPaletteSize)
    {
        _paletteSize = initialPaletteSize;   
        _chunks = new uint64[LogicSizeToRaw(initialCapacity)];
        _idToElement = new List<T>(initialPaletteSize);
        _elementToId = new Dictionary<T, int>(initialPaletteSize);
    }

    public T Get(int index)
    {
        if (index >= Capacity)
            throw new Exception($"index {index} is out of bound of capacity {Capacity}");

        var chunkIdx = index / ElementsPerChunk;
        var subIdx = index - chunkIdx * ElementsPerChunk;
        var chunk = _chunks[chunkIdx];
        chunk >>= subIdx * BitsPerElement();
        uint mask = (1U << BitsPerElement()) - 1;
        int id = (int) (chunk & mask);
        return _idToElement[id];
    }

    public void Set(int index, T value)
    {
        if (index >= Capacity)
            throw new Exception($"index {index} is out of bound of capacity {Capacity}");

        if (!_elementToId.TryGetValue(value, out var id))
        {
            Logger.Highlight(this, 
                $"{value} is not in palette, increasing capacity {Capacity} -> {Capacity*2}");
            
        }
    }
    
    public void EnsurePaletteCapacity(int newCap)
    {
        if (newCap <= Capacity) return;
        Array.Resize(ref _chunks, LogicSizeToRaw(newCap));
    }

    public void EnsureCapacity(int newCap)
    {
        if (newCap <= Capacity) return;
        Array.Resize(ref _chunks, LogicSizeToRaw(newCap));
    }

    public int LogicSizeToRaw(int logic)
    {
        if (logic == 0) return 0;
        return ((logic - 1) / ElementsPerChunk) +1; // division with ceiling
    }

    #region Private

    private int ElementsPerChunk => ChunkBitSize / BitsPerElement();

    private int Capacity => _chunks.Length / ElementsPerChunk;

    private int BitsPerElement()
    {
        if (_paletteSize <= 1) 
            return 1;
        return ChunkBitSize - BitOperations.LeadingZeroCount((uint)_paletteSize - 1);
    }

    #endregion
}