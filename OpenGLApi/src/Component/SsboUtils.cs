using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;

public static class SsboUtils
{
    private static readonly HashSet<Type> CheckedTypes = new();
    public static Error? CheckStd430<T>(bool isArray, int previousStructSize) where T : unmanaged
    {
        var type = typeof(T);
        if (!CheckedTypes.Add(type))
            return null;

        Logger.Info(typeof(SsboUtils),"-----------------------------------------");
        Logger.Info(typeof(SsboUtils),$"Checking for correct struct '{type.Name}' alignment:");

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
            Logger.Info(typeof(SsboUtils), 
                $"field {(field.Name + ';').PadRight(maxFieldName)}\t offset: {offset};\t size: {size}");

            if (offset % Alignment(size) != 0 && !field.Name.ToLower().Contains("padding"))
            {
                return $"field {field.Name} must be padding, else it won't be used";
            }
        }
        
        int structSize = Marshal.SizeOf<T>();
        Logger.Info(typeof(SsboUtils), $"total size: {structSize}");
        
        if (isArray && structSize % alignment != 0)
            return $"cause it is array, total struct size must be divisible by {alignment}, but got {structSize} % {alignment}";
        
        Logger.Info(typeof(SsboUtils), "check complete, everything seem correct");
        Logger.Info(typeof(SsboUtils), "-----------------------------------------");
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