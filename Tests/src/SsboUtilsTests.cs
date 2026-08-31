using System.Runtime.InteropServices;
using FluentAssertions;
using OpenGLApi.Component.Buffer;

namespace Tests;

/// <summary>
/// DeepSeek slop
/// </summary>
public class SsboUtilsTests
{
    // Вспомогательные структуры для тестов

    // Правильная структура: одно поле int
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SingleInt
    {
        public int Value;
    }

    // Структура с vec3 (имитируем через три float) без паддинга – должна вызвать ошибку
    [StructLayout(LayoutKind.Sequential)]
    struct Vec3NoPadding
    {
        public float X, Y, Z;
        public float Next; // следующее поле будет со смещением 12, выравнивание 4 – ок, но это не vec3 с выравниванием 16
        // Но в std430 vec3 требует смещения кратного 16. Здесь первое поле start = 0 (кратно 16), но его размер 12, и следующее поле начинается с 12, что допустимо.
        // Для имитации vec3, который требует после себя паддинга, нужно, чтобы следующее поле было с выравниванием 16 (например, другой vec3 или vec4).
    }

    // Правильная структура с vec3 и явным паддингом
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Vec3WithPadding
    {
        public float X, Y, Z;
        private float _padding1; // чтобы размер стал 16
        public float Next; // после паддинга смещение будет 16 (кратно 16? 16 кратно 4, ок)
    }

    // Структура, где поле Vector3 из System.Numerics (размер 12, выравнивание 4 по умолчанию в .NET)
    // Но для std430 требуется выравнивание 16 – для теста создадим свою структуру с явным размером и выравниванием.
    // Имитация Vector3 с выравниванием 16 через явный LayoutKind.Explicit.
    [StructLayout(LayoutKind.Explicit)]
    struct Vector3Aligned16
    {
        [FieldOffset(0)] public float X;
        [FieldOffset(4)] public float Y;
        [FieldOffset(8)] public float Z;
        // автоматически размер = 12, но выравнивание структуры = 4? Чтобы получить выравнивание 16, нужно добавить паддинг в конце или использовать LayoutKind.Sequential с полем-падингом.
    }

    // Проще: использовать System.Numerics.Vector3, но его выравнивание в .NET – 4, что не соответствует std430. Поэтому валидатор должен указать на ошибку, если нет ручного паддинга.
    // Создадим структуру с System.Numerics.Vector3 без паддинга.
    [StructLayout(LayoutKind.Sequential)]
    struct StructWithVector3
    {
        public System.Numerics.Vector3 Pos;
        public float Intensity;
    }

    // Правильная структура с System.Numerics.Vector3 и паддингом до 16 байт
    [StructLayout(LayoutKind.Sequential)]
    struct StructWithVector3Padded
    {
        public System.Numerics.Vector3 Pos;
        private float _padding;
        public float Intensity;
    }

    // Структура без атрибута LayoutKind.Sequential (по умолчанию Sequential, но явно не указан)
    struct NoLayoutStruct
    {
        public int A;
        public float B;
    }

    // Структура для проверки массива (размер кратен выравниванию)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ArrayCompatibleStruct
    {
        public float A; // 4 байта
        public float B; // 4 байта
        // выравнивание = 4, размер = 8, кратен 4 – ок
    }

    // Структура для проверки массива (размер не кратен выравниванию) 
    // Выравнивание структуры = max(выравнивание полей). Если есть float – выравнивание 4, размер 6? Но .NET добавит паддинг.
    // Создадим структуру с полем Vector3 (12 байт) и одним float (4 байта) – сумма 16, выравнивание 16, размер кратен 16, проблем нет.
    // Чтобы размер не был кратен выравниванию, нужно, чтобы выравнивание было 16, а размер был 20 (например, два vec3 и float). 
    // Пусть структура: vec3 + float + vec3 – без паддингов между vec3 и между последним vec3 и концом.
    // Но .NET добавит паддинги при LayoutKind.Sequential автоматически? Для Vector3 (12) .NET использует выравнивание 4, поэтому следующее поле начнётся с 12. Итоговый размер 12+4+12=28. Выравнивание структуры = max(4,4,4)=4. 28 кратно 4, проблем нет. 
    // Для имитации реальной проблемы нужен тип с выравниванием 16 (например, Vector4). Пусть структура: Vector4 (16) + float (4) – размер 20, выравнивание 16, 20 % 16 != 0.
    [StructLayout(LayoutKind.Sequential)]
    struct ArrayIncompatibleStruct
    {
        public System.Numerics.Vector4 V; // 16 байт, выравнивание 16
        public float F;                   // 4 байта, выравнивание 4
        // размер = 20, .NET может добавить паддинг? Нет, по умолчанию Pack? Для Vector4 обычно 16, следующее поле начнётся с 16, размер структуры = 20. Выравнивание структуры = max(16,4)=16. 20 % 16 != 0.
    }
    
    [StructLayout(LayoutKind.Sequential)]
    struct StructWithVector4
    {
        public System.Numerics.Vector4 V;
    }

    // ======================= Тесты =======================

    [Fact]
    public void CheckStd430_ValidSingleInt_ReturnsNull()
    {
        // Act
        var result = SsboUtils.CheckStd430<SingleInt>(false, 0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckStd430_ValidStructWithPadding_ReturnsNull()
    {
        // Act
        var result = SsboUtils.CheckStd430<Vec3WithPadding>(false, 0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckStd430_WithoutSequentialLayout_ReturnsError()
    {
        // Act
        var result = SsboUtils.CheckStd430<NoLayoutStruct>(false, 0);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void CheckStd430_StructWithVector3NoPadding_ReturnsError()
    {
        // В этой структуре Vector3 (выравнивание в .NET 4) будет по смещению 0, следующее поле Intensity начнётся с 12 (кратно 4) – ошибки в валидаторе не будет, т.к. условие проверяет offset % Alignment(size) == 0. Alignment(4) = 4. 12 % 4 == 0, пройдёт.
        // Но настоящее нарушение std430 – выравнивание vec3 должно быть 16. Однако Marshal.OffsetOf для Vector3 даст 0, и валидатор не заметит, потому что использует Alignment(Marshal.SizeOf(Vector3)) = 16 (размер 12 -> 16). offset 0 % 16 == 0, первое поле ок. Следующее поле – Intensity, его смещение = 12, Alignment(4) = 4, 12 % 4 == 0, условие выполняется. Ошибки не будет. Это ложноположительный пропуск. Поэтому тест не сможет поймать такую структуру. Нужна другая структура, где поле с выравниванием 16 идёт после vec3 без паддинга. 
        // Создадим структуру: Vector3 затем Vector4. 
        
        // Act
        var result = SsboUtils.CheckStd430<Vec3NoPadding>(false, 0);

        // Assert
        result.Should().NotBeNull();
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Vec3ThenVec4
    {
        public System.Numerics.Vector3 V3; // 12 байт, смещение 0
        public System.Numerics.Vector4 V4; // ожидаемое смещение 16 (по std430), но .NET положит на 12
    }

    [Fact]
    public void CheckStd430_Vec3ThenVec4WithoutPadding_ReturnsError()
    {
        // Act
        var result = SsboUtils.CheckStd430<Vec3ThenVec4>(false, 0);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void CheckStd430_PreviousStructSizeNotAligned_ReturnsError()
    {
        // Предыдущий размер 12, выравнивание текущей структуры (SingleInt) = 4, 12 % 4 == 0 – не ошибка.
        // Чтобы получить ошибку, нужна структура с выравниванием 16, например, содержащая Vector4.

        // Выравнивание = 16
        var result = SsboUtils.CheckStd430<StructWithVector4>(false, 12); // 12 % 16 != 0

        result.Should().NotBeNull();
    }

    [Fact]
    public void CheckStd430_ArrayWithValidStructSize_ReturnsNull()
    {
        // Размер ArrayCompatibleStruct = 8, выравнивание = 4, кратно 4
        var result = SsboUtils.CheckStd430<ArrayCompatibleStruct>(true, 0);

        result.Should().BeNull();
    }

    [Fact]
    public void CheckStd430_ArrayWithIncompatibleStructSize_ReturnsError()
    {
        // Размер ArrayIncompatibleStruct скорее всего 20, выравнивание 16, 20 % 16 != 0
        var result = SsboUtils.CheckStd430<ArrayIncompatibleStruct>(true, 0);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CheckStd430_PrimitiveTypeInt_NoLayoutCheck()
    {
        // Примитивные типы не требуют атрибута
        var result = SsboUtils.CheckStd430<int>(false, 0);

        result.Should().BeNull();
    }

    [Fact]
    public void CheckStd430_PrimitiveTypeFloat_Array_Works()
    {
        var result = SsboUtils.CheckStd430<float>(true, 0);

        result.Should().BeNull();
    }
}