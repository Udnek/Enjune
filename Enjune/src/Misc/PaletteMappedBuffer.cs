using System.Numerics;
using System.Text;

namespace Enjune.Misc;

using uint64 = UInt64;

public class PaletteMappedBuffer<T> where T : IEquatable<T>
{
    private const int ChunkBitSize = sizeof(uint64) * 8; // 64 basically
    
    private uint64[] _chunks;
    private readonly List<T> _idToElement;
    private readonly Dictionary<T, int> _elementToId;
    private int _paletteCapacity;

    #region Public
    
    public PaletteMappedBuffer(int minimalCapacity, int minPaletteCapacity)
    {
        if (minimalCapacity <= 0)
            throw new Exception($"{nameof(minimalCapacity)} must be > 0, got {minimalCapacity}");
        if (minPaletteCapacity <= 0)
            throw new Exception($"{nameof(minPaletteCapacity)} must be > 0, got {minPaletteCapacity}");
        _paletteCapacity = Math.Max(2, NextPow2(minPaletteCapacity));   
        _chunks = new uint64[LogicSizeToChunks(minimalCapacity, _paletteCapacity)];
        _idToElement = new List<T>(_paletteCapacity);
        _elementToId = new Dictionary<T, int>(_paletteCapacity);
    }

    public int Capacity => ChunksSizeToLogic(_chunks.Length, _paletteCapacity);

    public string BinaryForm(int nFirstElements)
    {
        var b = new StringBuilder();
        b.Append("[ ");
        for (int i = 0; i < nFirstElements; i++)
        {
            var value = Read(_chunks, i, _paletteCapacity);
            var binString = Convert.ToString(value, 2)
                .PadLeft(BitOperations.Log2((uint)_paletteCapacity), '0');
            b.Append(binString);
            b.Append(' ');
        }
        b.Append(']');
        return b.ToString();
    }
    
    public T this[int index]
    {
        get
        {
            if ((uint)index >= Capacity)
                throw new Exception($"index {index} is out of bound of capacity {Capacity}");
            
            var id = Read(_chunks, index, _paletteCapacity);
            if (id >= _idToElement.Count)
                throw new Exception($"element at index {index} has id {id} that out of bounds of palette size {_elementToId.Count}");
            return _idToElement[id];
        }
        set
        {
            if ((uint)index >= Capacity)
                throw new Exception($"index {index} is out of bound of capacity {Capacity}");
            
            if (!_elementToId.TryGetValue(value, out var id))
            {
                Logger.Logger.Debug(this, 
                    $"{value} is not in palette, adding");
                EnsurePaletteCapacity(_idToElement.Count + 1);
                id = _idToElement.Count;
                _idToElement.Add(value);
                _elementToId[value] = id;
            }
            Write(_chunks, index, _paletteCapacity, id);
        }
    }
    
    public void EnsurePaletteCapacity(int newPaletteCap)
    {
        newPaletteCap = NextPow2(newPaletteCap);
        if (BitsPerElement(newPaletteCap) <= BitsPerElement(_paletteCapacity))
            return;
        
        // growing
        var oldPaletteCap = _paletteCapacity;
        Logger.Logger.Debug(this, $"increasing palette capacity {oldPaletteCap} -> {newPaletteCap}");
        var size = Capacity;
        Array.Resize(ref _chunks, LogicSizeToChunks(size, newPaletteCap));
        for (int i = size - 1; i >= 0; i--) // iteration in reverse not to erase values
        {
            var val = Read(_chunks, i, oldPaletteCap);
            Write(_chunks, i, newPaletteCap, val);
        }
        _paletteCapacity = newPaletteCap;
    }

    public void EnsureCapacity(int newCap)
    {
        if (newCap <= Capacity) 
            return;
        Array.Resize(ref _chunks, LogicSizeToChunks(newCap, _paletteCapacity));
    }

    #endregion
    
    #region Private
    
    private static int NextPow2(int n) => 1 << (int)Math.Ceiling(Math.Log2(n));
    
    private static int Read(uint64[] chunks, int logicIndex, int paletteCap)
    {
        var elemPerChunk = ElementsPerChunk(paletteCap);
        var bitsPerElem = BitsPerElement(paletteCap);
        
        var chunkIdx = logicIndex / elemPerChunk;
        var subIdx = logicIndex % elemPerChunk;
        var chunk = chunks[chunkIdx];
        chunk >>= subIdx * bitsPerElem;
        uint64 mask = (1U << bitsPerElem) - 1;
        int id = (int) (chunk & mask);
        return id;
    }

    private static void Write(uint64[] chunks, int logicIndex, int paletteCap, int idValue)
    {
        var elemPerChunk = ElementsPerChunk(paletteCap);
        var bitsPerElem = BitsPerElement(paletteCap);

        var chunkIdx = logicIndex / elemPerChunk;
        var subIdx = logicIndex % elemPerChunk;
        var chunk = chunks[chunkIdx];
        uint64 mask = (1U << bitsPerElem) - 1; // mask with ones
        mask <<= subIdx * bitsPerElem; // moving mask to index;
        chunk &= ~mask; // setting zeroes in chunk at mask
        chunk |= ((uint64)idValue) << (subIdx * bitsPerElem); // setting idValue
        chunks[chunkIdx] = chunk;
    }
    
    private static int ChunksSizeToLogic(int chunks, int paletteCapacity) 
        => chunks * ElementsPerChunk(paletteCapacity);

    private static int LogicSizeToChunks(int logicSize, int paletteCapacity)
    {
        if (logicSize == 0) 
            return 0;
        return ((logicSize - 1) / ElementsPerChunk(paletteCapacity)) +1; // division with ceiling
    }

    private static int ElementsPerChunk(int paletteCap) => ChunkBitSize / BitsPerElement(paletteCap);
    
    private static int BitsPerElement(int paletteCap) 
        => ChunkBitSize - BitOperations.LeadingZeroCount((uint64)paletteCap - 1);

    #endregion
}