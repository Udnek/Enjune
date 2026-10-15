using System.Text;
using Enjune.Graphic.Api.Data;
using OpenGLApi.Mesh;

namespace OpenGLApi.Component;

public static class ShaderGenerator
{
    private static void VertexIn(StringBuilder builder, ReadOnlySpan<VaoAttribute> attributes)
    {
        foreach (var attr in attributes)
        {
            // layout(location = 0) in vec3 pos;
            builder.Append($"layout(location = {attr.Location}) in {ToGlName(attr.Type)} {attr.InShaderName};");
        }
    }

    private static void VertexInputStruct(StringBuilder builder, ReadOnlySpan<VaoAttribute> attributes)
    {
        // struct Input {
        builder.Append("struct Input {");
        foreach (var field in attributes)
        {
            //      vec4 color;
            builder.Append($"   {ToGlName(field.Type)} {field.InShaderName};");
        }
        // };
        builder.Append("};");
    }
    
    private static void VaryingsOut(StringBuilder builder, BufferDesc varyings)
    {
        for (var i = 0; i < varyings.StructFields.Count; i++)
        {
            var varying = varyings.StructFields[i];
            // layout(location = 0) out vec4 color;
            builder.Append($"layout(location = {i}) out {ToGlName(varying.Type)} {varying.InShaderName};");
        }
    }

    private static void VaryingsStruct(StringBuilder builder, BufferDesc varyings)
    {
        // struct Varyings {
        builder.Append($"struct {varyings.InShaderName} {{");
        foreach (var field in varyings.StructFields)
        {
            //      vec4 color;
            builder.Append($"   {ToGlName(field.Type)} {field.InShaderName};");
        }
        // };
        builder.Append("};");
    }

    private static void ProcessVertexCall(StringBuilder builder, BufferDesc varyings)
    {
        // Varyings varyings = {};
        builder.Append($"{varyings.InShaderName} varyings = {{}};");
        // calling user's process
        builder.Append("process(input, varyings);");
        
        // decomposing result
        foreach (var field in varyings.StructFields)
        {
            // color = varyings.color;
            builder.Append($"{field.InShaderName} = varyings.{field.InShaderName};");
        }
    }
    
    private static void ProcessFragmentCall(StringBuilder builder, BufferDesc varyings)
    {
        // Varyings varyings = {};
        builder.Append("Varyings varyings = {};");
        // composing result
        foreach (var field in varyings.StructFields)
        {
            // varyings.color = color;
            builder.Append($"varyings.{field.InShaderName} = {field.InShaderName};");
        }
        // calling user's process
        builder.Append("vec4 color = process(varyings);");
        builder.Append("fragColor = color;");
    }
    
    private static void VaryingsIn(StringBuilder builder, BufferDesc varyings)
    {
        for (var i = 0; i < varyings.StructFields.Count; i++)
        {
            var varying = varyings.StructFields[i];
            // layout(location = 0) in vec4 color;
            builder.Append($"layout(location = {i}) in {ToGlName(varying.Type)} {varying.InShaderName};");
        }
    }

    private static void Ssbos(StringBuilder builder, ReadOnlySpan<BufferDesc> ssbos)
    {
        const string structPostfix = "_struct";
        const string bufferPostfix = "_buff";
        foreach (var ssbo in ssbos)
        {
            // struct PerPrim_struct {
            builder.Append($"struct {ssbo.InShaderName}{structPostfix} {{");
            foreach (var field in ssbo.StructFields)
            {
                //      int matId;
                builder.Append($"   {ToGlName(field.Type)} {field.InShaderName};");
            }
            // };
            builder.Append("};");
            
            // layout(std430, binding = 0) readonly buffer PerPrim_buff {
            builder.Append($"layout(std430, binding = {ssbo.Binding}) readonly buffer {ssbo.InShaderName}{bufferPostfix} {{");
            //     PerPrim_struct PerPrim[];
            builder.Append($"    {ssbo.InShaderName}{structPostfix} {ssbo.InShaderName}[];");
            // };
            builder.Append("};");
        }
    }

    private static void Ubos(StringBuilder builder, ReadOnlySpan<BufferDesc> ubos)
    {
        foreach (var ubo in ubos)
        {
            // layout(std140, binding = 0) uniform CameraUniforms {
            builder.Append($"layout(std140, binding = {ubo.Binding}) uniform {ubo.InShaderName} {{");
            foreach (var field in ubo.StructFields)
            {
                //     mat4 View;
                builder.Append($"    {ToGlName(field.Type)} {field.InShaderName};");
            }
            // };
            builder.Append("};");
        }
    }
    
    public static string CreateVertex(        
        ReadOnlySpan<VaoAttribute> attributes,
        ReadOnlySpan<BufferDesc> ssbos,
        ReadOnlySpan<BufferDesc> ubos,
        BufferDesc varyings)
    {
        var builder = new StringBuilder();
        // header
        builder.Append("#version 430 core");
        
        Ssbos(builder, ssbos);
        Ubos(builder, ubos);
        VaryingsStruct(builder, varyings);
        
        VertexIn(builder, attributes);
        VaryingsOut(builder, varyings);
        
        ProcessVertexCall(builder, varyings);
        
        return builder.ToString();
    }

    public static string CreateFragment(        
        ReadOnlySpan<BufferDesc> ssbos,
        ReadOnlySpan<BufferDesc> ubos,
        BufferDesc varyings)
    {
        var builder = new StringBuilder();
        // header
        builder.Append("#version 430 core");

        Ssbos(builder, ssbos);
        Ubos(builder, ubos);
        VaryingsStruct(builder, varyings);
        
        VaryingsIn(builder, varyings);
        builder.Append("out vec4 fragColor;");
        
        ProcessFragmentCall(builder, varyings);

        return builder.ToString();
    }
    
    public static string Create(
        ShaderType shaderType,
        ReadOnlySpan<VaoAttribute> attributes,
        ReadOnlySpan<BufferDesc> ssbos,
        ReadOnlySpan<BufferDesc> ubos,
        BufferDesc varyings)
    {
        var builder = new StringBuilder();
        
        // header
        builder.Append("#version 430 core");

        Ssbos(builder, ssbos);
        Ubos(builder, ubos);
        VaryingsStruct(builder, varyings);
        
        if (shaderType == ShaderType.VertexShader)
        {
            VertexIn(builder, attributes);
            VaryingsOut(builder, varyings);
        } 
        else if (shaderType == ShaderType.FragmentShader)
        {
            VaryingsIn(builder, varyings);
            // fragColor
            builder.Append("out vec4 fragColor;");
            ProcessFragmentCall(builder, varyings);
        }

        return builder.ToString();
    }
    
    private static string ToGlName(ShaderValuePrimitive primitive)
    {
        return primitive switch {
            ShaderValuePrimitive.Int => "int",
            ShaderValuePrimitive.Float => "float",
            ShaderValuePrimitive.Vector2F => "vec2",
            ShaderValuePrimitive.Vector3F => "vec3",
            ShaderValuePrimitive.Vector4F => "vec4",
            ShaderValuePrimitive.Matrix3F => "mat3",
            ShaderValuePrimitive.Matrix4F => "mat4",
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
    }
}