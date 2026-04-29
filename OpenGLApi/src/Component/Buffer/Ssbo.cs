using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;

public class Ssbo<T> : AbstractBuffer<T> where T : unmanaged
{
    private readonly int _binding;

    public Ssbo(int binding, int capacity, T[]? initialData = null) : base(BufferTarget.ShaderStorageBuffer, capacity, initialData)
    {
        _binding = binding;
        var type = typeof(T);
        Logger.Log(this,"check for correct struct padding:");
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var isOk = true;
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            
            var offset = (int) Marshal.OffsetOf<T>(field.Name);
            var size = Marshal.SizeOf(field.FieldType);
            Logger.Log(this, $"field {field.Name};\t offset: {offset};\t size: {size}");
            
            if (!(size == 12 && !field.Name.ToLower().Contains("padding"))) continue;
            // probably should add 4 more bytes (std430)
            
            if (i < fields.Length - 1 && fields[i + 1].Name.ToLower().Contains("padding")) continue;
            // padding not found
            
            isOk = false;
            Logger.Error(this, "padding field with 'padding' in name not found");
        }

        if (isOk)
        {
            var maxSize = fields.Select(f => Marshal.SizeOf(f.FieldType)).Max();
            if (Marshal.SizeOf(type) % maxSize != 0)
            {
                isOk = false;
                Logger.Error(this, $"total struct size must be multiple of max alignment; {Marshal.SizeOf(type)} % {maxSize} != 0");
            }
        }
        
        if (isOk) 
            Logger.Log(this, "check complete, everything seems correct");
        
        Bind();
    }

    // todo do something with 'new' hiding
    public new void Bind()
    {
        base.Bind();
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, _binding, Handle);
    }
}