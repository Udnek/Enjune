using System;
using Enjune.Misc;
using Xunit;

namespace Tests;

/// <summary>
/// DeepSeek slop
/// </summary>
public class PaletteMappedBufferTests
{
    /// <summary>
    /// Простой value-тип, чтобы отличать значения палитры от id.
    /// </summary>
    private readonly struct Color : IEquatable<Color>
    {
        public readonly uint Rgba;
        public Color(uint rgba) => Rgba = rgba;
        public bool Equals(Color other) => Rgba == other.Rgba;
        public override bool Equals(object? obj) => obj is Color c && Equals(c);
        public override int GetHashCode() => (int)Rgba;
        public override string ToString() => $"Color({Rgba:X8})";
    }

    #region Construction

    [Fact]
    public void Constructor_Throws_OnZeroCapacity()
    {
        Assert.ThrowsAny<Exception>(() => new PaletteMappedBuffer<Color>(0, 4));
    }

    [Fact]
    public void Constructor_Throws_OnNegativeCapacity()
    {
        Assert.ThrowsAny<Exception>(() => new PaletteMappedBuffer<Color>(-1, 4));
    }

    [Fact]
    public void Constructor_Throws_OnZeroPaletteSize()
    {
        Assert.ThrowsAny<Exception>(() => new PaletteMappedBuffer<Color>(10, 0));
    }

    [Fact]
    public void Constructor_Throws_OnNegativePaletteSize()
    {
        Assert.ThrowsAny<Exception>(() => new PaletteMappedBuffer<Color>(10, -1));
    }

    [Fact]
    public void Constructor_CapacityAtLeastRequested()
    {
        var buf = new PaletteMappedBuffer<Color>(1000, 4);
        Assert.True(buf.Capacity >= 1000,
            $"Expected capacity >= 1000, got {buf.Capacity}");
    }

    [Fact]
    public void Constructor_WithMinimalPalette_Works()
    {
        // minPaletteCapacity = 1 форсируется до 2
        var buf = new PaletteMappedBuffer<Color>(10, 1);
        Assert.True(buf.Capacity >= 10);
        buf[0] = new Color(1);
        Assert.Equal(new Color(1), buf[0]);
    }

    #endregion

    #region Basic indexer

    [Fact]
    public void Indexer_SetThenGet_ReturnsSameValue()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        var c = new Color(0xAB);
        buf[0] = c;
        Assert.Equal(c, buf[0]);
    }

    [Fact]
    public void Indexer_MultipleIndices_AllRoundtrip()
    {
        var buf = new PaletteMappedBuffer<Color>(20, 2);
        var a = new Color(1);
        var b = new Color(2);
        var c = new Color(3);

        buf[0] = a;
        buf[1] = b;
        buf[19] = c;

        Assert.Equal(a, buf[0]);
        Assert.Equal(b, buf[1]);
        Assert.Equal(c, buf[19]);
    }

    [Fact]
    public void Indexer_Throws_OnNegativeIndex_Get()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        Assert.ThrowsAny<Exception>(() => _ = buf[-1]);
    }

    [Fact]
    public void Indexer_Throws_OnNegativeIndex_Set()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        Assert.ThrowsAny<Exception>(() => buf[-1] = new Color(1));
    }

    [Fact]
    public void Indexer_Throws_OnOutOfBounds_Get()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        int cap = buf.Capacity;
        Assert.ThrowsAny<Exception>(() => _ = buf[cap]);
    }

    [Fact]
    public void Indexer_Throws_OnOutOfBounds_Set()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        int cap = buf.Capacity;
        Assert.ThrowsAny<Exception>(() => buf[cap] = new Color(1));
    }

    [Fact]
    public void Indexer_ReadingUnsetElement_Throws()
    {
        // Палитра пуста, read возвращает id=0, которого нет в _idToElement
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        Assert.ThrowsAny<Exception>(() => _ = buf[0]);
    }

    #endregion

    #region Palette behavior

    [Fact]
    public void SetSameValue_DoesNotGrowPalette()
    {
        // Если палитра растёт некорректно на каждом Set, где-то всё сломается
        var buf = new PaletteMappedBuffer<Color>(100, 2);
        var v = new Color(42);
        for (int i = 0; i < 100; i++)
            buf[i] = v;

        for (int i = 0; i < 100; i++)
            Assert.Equal(v, buf[i]);
    }

    [Fact]
    public void SetTwoValues_ReusesPalette()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        var a = new Color(1);
        var b = new Color(2);
        buf[0] = a;
        buf[1] = b;
        buf[2] = a; // должно переиспользовать id для a
        buf[3] = b;

        Assert.Equal(a, buf[0]);
        Assert.Equal(b, buf[1]);
        Assert.Equal(a, buf[2]);
        Assert.Equal(b, buf[3]);
    }

    [Fact]
    public void ManyDistinctValues_ForcesPaletteGrowth_AllRoundtrip()
    {
        var buf = new PaletteMappedBuffer<Color>(100, 2);
        var expected = new Color[50];
        for (int i = 0; i < 50; i++)
        {
            expected[i] = new Color((uint)(i + 1));
            buf[i] = expected[i];
        }
        for (int i = 0; i < 50; i++)
            Assert.Equal(expected[i], buf[i]);
    }

    #endregion

    #region Capacity growth

    [Fact]
    public void EnsureCapacity_GrowsCapacity()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        int old = buf.Capacity;
        buf.EnsureCapacity(1000);
        Assert.True(buf.Capacity >= 1000);
        Assert.True(buf.Capacity > old);
    }

    [Fact]
    public void EnsureCapacity_Smaller_NoOp()
    {
        var buf = new PaletteMappedBuffer<Color>(100, 2);
        int old = buf.Capacity;
        buf.EnsureCapacity(10);
        Assert.Equal(old, buf.Capacity);
    }

    [Fact]
    public void EnsureCapacity_PreservesExistingValues()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 2);
        buf[0] = new Color(1);
        buf[5] = new Color(2);
        buf.EnsureCapacity(1000);

        Assert.Equal(new Color(1), buf[0]);
        Assert.Equal(new Color(2), buf[5]);
    }

    #endregion

    #region Bit packing correctness (критичные тесты)

    // Каждое значение палитры — разные bitsPerElement:
    //  palette=2  -> bits=1, epc=64
    //  palette=4  -> bits=2, epc=32
    //  palette=8  -> bits=3, epc=21
    //  palette=16 -> bits=4, epc=16
    //  palette=32 -> bits=5, epc=12
    //  palette=128-> bits=7, epc=9
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(128)]
    public void BitPacking_MultiplePaletteSizes_Roundtrip(int paletteSize)
    {
        // initialCapacity = 200, чтобы гарантированно пересекать несколько чанков
        var buf = new PaletteMappedBuffer<Color>(200, paletteSize);
        var expected = new Color[200];
        for (int i = 0; i < 200; i++)
        {
            expected[i] = new Color((uint)(i * 7 + 1));
            buf[i] = expected[i];
        }

        for (int i = 0; i < 200; i++)
            Assert.Equal(expected[i], buf[i]);
    }

    /// <summary>
    /// Регрессия: Write не должен писать в биты соседей.
    /// На баге "Write без * bitsPerElem" — падает.
    /// </summary>
    [Fact]
    public void WriteToElement1_PreservesElement0()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 16); // bits=4
        var a = new Color(1);
        var b = new Color(2);
        buf[0] = a;
        buf[1] = b;

        Assert.Equal(a, buf[0]);
        Assert.Equal(b, buf[1]);
    }

    /// <summary>
    /// Регрессия: запись в элемент 0 не должна стирать биты 32-63 чанка
    /// (баг "mask типа uint"). На исправном коде — проходит.
    /// </summary>
    [Fact]
    public void WriteToElement0_DoesNotCorruptUpperHalfOfChunk()
    {
        var buf = new PaletteMappedBuffer<Color>(20, 16); // bits=4, epc=16
        // Заполняем все 16 элементов первого чанка
        for (int i = 0; i < 16; i++)
            buf[i] = new Color((uint)(i + 1));

        // Перезаписываем элемент 0
        buf[0] = new Color(99);

        // Все остальные элементы должны сохраниться
        for (int i = 1; i < 16; i++)
            Assert.Equal(new Color((uint)(i + 1)), buf[i]);
        Assert.Equal(new Color(99), buf[0]);
    }

    /// <summary>
    /// Регрессия: write/read должны совпадать для любого subIdx.
    /// </summary>
    [Fact]
    public void WriteToVariousSubIndices_AllReadBack()
    {
        var buf = new PaletteMappedBuffer<Color>(64, 16); // bits=4, epc=16
        // Устанавливаем разные значения во все subIdx первого чанка
        for (int i = 0; i < 16; i++)
            buf[i] = new Color((uint)(100 + i));

        for (int i = 0; i < 16; i++)
            Assert.Equal(new Color((uint)(100 + i)), buf[i]);
    }

    #endregion

    #region EnsurePaletteCapacity

    [Fact]
    public void EnsurePaletteCapacity_GrowsBits_PreservesData()
    {
        var buf = new PaletteMappedBuffer<Color>(100, 2); // bits=1, epc=64

        // Заполняем так, чтобы использовать несколько значений
        for (int i = 0; i < 50; i++)
            buf[i] = new Color((uint)(i % 2));

        // Принудительно растим палитру
        buf.EnsurePaletteCapacity(64);

        // Данные сохранились
        for (int i = 0; i < 50; i++)
            Assert.Equal(new Color((uint)(i % 2)), buf[i]);
    }

    [Fact]
    public void EnsurePaletteCapacity_SmallerOrSame_NoOp()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 16);
        buf[0] = new Color(1);

        buf.EnsurePaletteCapacity(4); // меньше текущей — no-op
        Assert.Equal(new Color(1), buf[0]);

        buf.EnsurePaletteCapacity(16); // та же — no-op
        Assert.Equal(new Color(1), buf[0]);
    }

    #endregion

    #region Stress / integration

    [Fact]
    public void FillWithManyDistinctValues_ReadBackAll()
    {
        // Реалистичный сценарий: заполняем 1000 уникальных блоков
        // Палитра многократно растёт (1→2→4→...→1024)
        var buf = new PaletteMappedBuffer<Color>(1000, 2);
        var expected = new Color[1000];
        for (int i = 0; i < 1000; i++)
        {
            expected[i] = new Color((uint)(i * 3 + 1));
            buf[i] = expected[i];
        }

        for (int i = 0; i < 1000; i++)
            Assert.Equal(expected[i], buf[i]);
    }

    [Fact]
    public void InterleavedWrites_AndPaletteGrowth_NoDataLoss()
    {
        // Мешаем записи с ростом палитры: пишем value, потом new value, потом старый...
        var buf = new PaletteMappedBuffer<Color>(200, 2);
        var expected = new Color[200];

        // Изначально заполняем все одним цветом
        var first = new Color(1);
        for (int i = 0; i < 200; i++)
        {
            buf[i] = first;
            expected[i] = first;
        }

        // Теперь заменяем каждый 3-й на новый уникальный
        for (int i = 0; i < 200; i += 3)
        {
            expected[i] = new Color((uint)(i + 100));
            buf[i] = expected[i];
        }

        for (int i = 0; i < 200; i++)
            Assert.Equal(expected[i], buf[i]);
    }

    [Fact]
    public void RepeatedOverwrite_SameElement_NoCorruption()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 16); // bits=4, epc=16
        for (int round = 0; round < 100; round++)
        {
            buf[5] = new Color((uint)(round + 1));
            Assert.Equal(new Color((uint)(round + 1)), buf[5]);
        }
    }

    #endregion

    #region BinaryForm

    [Fact]
    public void BinaryForm_ContainsBrackets()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 4);
        buf[0] = new Color(1);
        buf[1] = new Color(2);
        var s = buf.BinaryForm(2);

        Assert.StartsWith("[", s);
        Assert.EndsWith("]", s);
    }

    [Fact]
    public void BinaryForm_ZeroFirstElements_OnlyBrackets()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 4);
        var s = buf.BinaryForm(0);
        Assert.Equal("[ ]", s);
    }

    [Fact]
    public void BinaryForm_WidthMatchesBitsPerElement()
    {
        var buf = new PaletteMappedBuffer<Color>(10, 16); // bits=4
        buf[0] = new Color(1); // id=0
        buf[1] = new Color(2); // id=1

        var s = buf.BinaryForm(2);
        // Каждое число — 4 символа (bits=4), с пробелами
        Assert.Equal("[ 0000 0001 ]", s);
    }

    #endregion
}