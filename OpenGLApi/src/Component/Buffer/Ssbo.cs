using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;


public sealed class SsboDataAndArray<TData, TArray> : GlDisposable where TArray : unmanaged where TData : unmanaged
{
    private readonly int _binding;
    private readonly int _handle;
    private readonly int _dataSize;
    private readonly int _arrayElementSize;
    public int ArrayCapacity { get; private set; }
    public readonly bool Final;

    public TData CurrentData = new();

    public SsboDataAndArray(int binding, int arrayCapacity, bool final)
    {
        _binding = binding;
        Final = final;
        if (arrayCapacity <= 0)
        {
            Logger.Error(this, "array capacity must be positive");
            arrayCapacity = 1;
        }
        ArrayCapacity = arrayCapacity;
        _handle = GL.GenBuffer();
        unsafe
        {
            _arrayElementSize = sizeof(TArray);
            _dataSize = sizeof(TData);
        }
        SsboUtils.CheckStd430<TData>(false, 0)?.Log(this);
        SsboUtils.CheckStd430<TArray>(true, Marshal.SizeOf<TData>())?.Log(this);
        Bind();
        if (final) 
            GL.BufferStorage(BufferTarget.ShaderStorageBuffer, _dataSize + arrayCapacity*_arrayElementSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        else 
            GL.BufferData(BufferTarget.ShaderStorageBuffer, _dataSize + arrayCapacity*_arrayElementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
    }

    public void Reallocate(int newArrayCapacity)
    {
        if (Final)
        {
            Logger.Error(this, "trying to reallocate final buffer");
            return;
        }
        if (newArrayCapacity <= 0)
        {
            Logger.Error(this, "array capacity must be positive");
            newArrayCapacity = 1;
        }
        Bind();
        GL.BufferData(BufferTarget.ShaderStorageBuffer, _dataSize + newArrayCapacity*_arrayElementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        Logger.Log(this, $"array capacity increased: {ArrayCapacity} -> {newArrayCapacity}");
        ArrayCapacity = newArrayCapacity;
    }
    
    
    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _handle);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, _binding, _handle);
    }

    public void BindAndPush(TData data, TArray[] array)
    {
        Bind();
        CurrentData = data;
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, _dataSize, ref data);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, _dataSize, array.Length*_arrayElementSize, array);
    }
    
    public void BindAndPushData(TData data)
    {
        Bind();
        CurrentData = data;
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, _dataSize, ref data);
    }

    protected override void DisposeGlData() => GL.DeleteBuffer(_handle);
}

public class SsboArray<T> : AbstractBuffer<T> where T : unmanaged
{
    private readonly int _binding;

    public SsboArray(int binding, int capacity, bool final, T[]? initialData = null) 
        : base(BufferTarget.ShaderStorageBuffer, capacity, final, initialData)
    {
        _binding = binding;
        SsboUtils.CheckStd430<T>(true, 0)?.Log(this);
        Bind();
    }

    // todo do something with 'new' hiding
    public new void Bind()
    {
        base.Bind();
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, _binding, Handle);
    }
}

public static class SsboUtils
{
    public static Error? CheckStd430<T>(bool isArray, int previousStructSize) where T : unmanaged
    {
        // TODO ADD CACHING FOR CHECKED TYPES
        var type = typeof(T);
        Logger.Log(typeof(SsboUtils),"-----------------------------------------");
        Logger.Log(typeof(SsboUtils),$"checking for correct struct '{type.Name}' alignment:");

        if (!type.IsPrimitive)
        {
            var structLayout = type.StructLayoutAttribute;
            if (structLayout == null || structLayout.Value != LayoutKind.Sequential || structLayout.Pack != 1)
                return $"struct '{type.Name}' must have [StructLayout(LayoutKind.Sequential), Pack = 1] attribute";
        }
        
        var alignment = Alignment<T>();
        if (previousStructSize % alignment != 0)
            return $"struct must begin with offset % {alignment} = 0, but got: {previousStructSize} % {alignment}";

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        int maxFieldName = fields.Max(f => f.Name.Length)+1; // +1 cause ';'
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            
            var offset = (int) Marshal.OffsetOf<T>(field.Name) + previousStructSize;
            var size = Marshal.SizeOf(field.FieldType);
            Logger.Log(typeof(SsboUtils), 
                $"field {(field.Name + ';').PadRight(maxFieldName)}\t offset: {offset};\t size: {size}");

            if (offset % Alignment(size) != 0 && !field.Name.ToLower().Contains("padding"))
            {
                return $"field {field.Name} must be padding, else it won't be used";
            }
        }
        
        int structSize = Marshal.SizeOf<T>();
        Logger.Log(typeof(SsboUtils), $"total size: {structSize}");
        
        if (isArray && structSize % alignment != 0)
            return $"cause it is array, total struct size must be divisible by {alignment}, but got {structSize} % {alignment}";
        
        Logger.Log(typeof(SsboUtils), "check complete, everything seem correct");
        Logger.Log(typeof(SsboUtils), "-----------------------------------------");
        return null;
    }

    public static int Alignment(int fieldSize)
    {
        return fieldSize switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 4 => 4,
            <= 8 => 8,
            _ => 16
        };
    }

    // alignment of struct is maximum of its fields;
    public static int Alignment<T>()
    {
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return fields.Max(f => Alignment(Marshal.SizeOf(f.FieldType)));
    }
}