using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using Enjune.Misc;
using OpenGLApi.Mesh;

namespace OpenGLApi.Component;

public abstract class Std
{
    public static readonly Std Std140 = new Std140Impl();
    public static readonly Std Std430 = new Std430Impl();
    
    private readonly Dictionary<Type, Error?> _cached = [];

    protected abstract Error? CheckSize(int size, int maxAlign);

    public Error? Check<T>() where T : unmanaged
    {
        if (_cached.TryGetValue(typeof(T), out var cachedErr))
            return cachedErr;

        var err = CheckNoCache<T>();
        _cached[typeof(T)] = err;
        return err;
    }

    private Error? CheckNoCache<T>() where T : unmanaged
    {
        Logger.Info(this,
            $"Validating {Logger.GetTypeName<T>()}");

        var type = typeof(T);
        if (type.IsPrimitive)
            return $"can not use primitive {Logger.GetTypeName(type)}";

        var layout = type.StructLayoutAttribute;
        if (layout is null)
            return $"struct must has {Logger.GetTypeName<StructLayoutAttribute>()}";

        if (layout.Value != LayoutKind.Explicit)
            return $"layoutKind must be {nameof(LayoutKind.Explicit)}, got: {layout.Value}";
        
        if (layout.Size <= 0)
            return $"size must be positive, got {layout.Size}";
        
        // fields
        var fields = type
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(fi => (fi, fi.GetCustomAttribute<FieldOffsetAttribute>()!.Value))
            // ordering by offset, cause GetFields order is not guaranteed;
            .OrderBy(t => t.Value).ToArray();
        
        int currentOffset = 0;
        int maxAlign = 0;
        foreach (var field in fields)
        {
            var fieldOffset = field.Value;
            var fieldSize = Marshal.SizeOf(field.fi.FieldType);
            var fieldAlign = Alignment(fieldSize);
            currentOffset = Align(currentOffset, fieldAlign);
            
            if (currentOffset != fieldOffset)
                return $"expected offset for field {field.fi.FieldType} to be {currentOffset}, got: {fieldOffset}";
            
            maxAlign = Math.Max(maxAlign, fieldAlign);
            currentOffset += fieldSize;
        }

        if (layout.Size < currentOffset)
            return $"size is to small to fit, must be at least {currentOffset}, got {layout.Size}";
        
        var sizeErr = CheckSize(layout.Size, maxAlign);
        return sizeErr;
    }

    private static int Align(int offset, int alignment)
    {
        if (offset % alignment == 0) return offset;
        return (offset / alignment + 1) * alignment;
    }
    
    private static int Alignment(int size)
    {
        return size switch {
            <= 4 => 4,
            <= 8 => 8,
            _ => 16
        };
    }
    
    
    
    private sealed class Std430Impl : Std
    {
        protected override Error? CheckSize(int size, int maxAlign)
        {
            if (size % maxAlign == 0) return null;
            return $"size % maxAlign must be 0, got size={size}, maxAlign={maxAlign}";
        }
    }

    private sealed class Std140Impl : Std
    {
        protected override Error? CheckSize(int size, int maxAlign)
        {
            if (size % 16 == 0) return null;
            return $"size % 16 must be 0, got size={size}";
        }
    }
}