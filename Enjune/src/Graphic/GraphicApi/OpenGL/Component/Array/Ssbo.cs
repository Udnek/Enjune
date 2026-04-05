using System.Reflection;
using System.Runtime.InteropServices;
using Enjune.Misc;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Array;

public class Ssbo<T> : AbstractBuffer<T> where T : unmanaged
{
    public Ssbo(int binding, int capacity) : base(BufferTarget.ShaderStorageBuffer, capacity)
    {
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

        
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, Handle);
    }
}