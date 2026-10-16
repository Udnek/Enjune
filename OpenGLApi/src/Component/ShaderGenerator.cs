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
            builder.AppendLine($"layout(location = {attr.Location}) in {ToGlName(attr.Type)} {attr.InShaderName}_in;");
        }
    }
    
    private static void VaryingsOut(StringBuilder builder, StructDesc varyings)
    {
        for (var i = 0; i < varyings.Fields.Length; i++)
        {
            var varying = varyings.Fields[i];
            // layout(location = 0) out vec4 color;
            builder.AppendLine($"layout(location = {i}) out {ToGlName(varying.Type)} {varying.InShaderName}_var;");
        }
    }

    private static void VaryingsStruct(StringBuilder builder, StructDesc varyings)
    {
        // struct Varyings {
        builder.AppendLine("struct Varyings {");
        foreach (var field in varyings.Fields)
        {
            //      vec4 color;
            builder.AppendLine($"\t{ToGlName(field.Type)} {field.InShaderName};");
        }
        // };
        builder.AppendLine("};");
    }

    private static void FragmentInputStruct(StringBuilder builder, ReadOnlySpan<VaoAttribute> attributes)
    {
        // struct Input {
        builder.AppendLine("struct Input {");
        foreach (var attr in attributes)
        {
            //      vec4 color;
            builder.AppendLine($"\t{ToGlName(attr.Type)} {attr.InShaderName};");
        }
        // };
        builder.AppendLine("};");
    }
    
    private static void VertexMain(StringBuilder builder, StructDesc varyings, ReadOnlySpan<VaoAttribute> attributes)
    {
        builder.AppendLine("void main() {");
        builder.AppendLine("\tInput input;");
        foreach (var attr in attributes)
        {
            builder.AppendLine($"\tinput.{attr.InShaderName} = {attr.InShaderName}_in;");
        }
        builder.AppendLine("\tVaryings varyings;");
        builder.AppendLine("\tgl_Position = process(input, varyings);");
        // decomposing result
        foreach (var field in varyings.Fields)
        {
            // color = varyings.color;
            builder.AppendLine($"\t{field.InShaderName}_var = varyings.{field.InShaderName};");
        }
        builder.AppendLine("}");
    }
    
    private static void FragmentMain(StringBuilder builder, StructDesc varyings)
    {
        builder.AppendLine("void main() {");
        builder.AppendLine("\tVaryings varyings;");
        // composing result
        foreach (var field in varyings.Fields)
        {
            // varyings.color = color;
            builder.AppendLine($"\tvaryings.{field.InShaderName} = {field.InShaderName}_in;");
        }
        // calling user's process
        builder.AppendLine("\tfragColor = process(varyings, PerPrimitives[gl_PrimitiveID]);");
        builder.AppendLine("}");
    }
    
    private static void VaryingsIn(StringBuilder builder, StructDesc varyings)
    {
        for (var i = 0; i < varyings.Fields.Length; i++)
        {
            var varying = varyings.Fields[i];
            // layout(location = 0) in vec4 color;
            builder.AppendLine($"layout(location = {i}) in {ToGlName(varying.Type)} {varying.InShaderName}_in;");
        }
    }

    private static void Ssbos(StringBuilder builder, ReadOnlySpan<BufferDesc> ssbos)
    {
        foreach (var ssbo in ssbos)
        {
            // struct PerPrim_struct {
            builder.AppendLine($"struct {ssbo.StructName} {{");
            foreach (var field in ssbo.StructFields)
            {
                //      int matId;
                builder.AppendLine($"\t{ToGlName(field.Type)} {field.InShaderName};");
            }
            // };
            builder.AppendLine("};");
            
            // layout(std430, binding = 0) readonly buffer PerPrim_buff {
            builder.AppendLine($"layout(std430, binding = {ssbo.Binding}) readonly buffer {ssbo.StructName}_buff {{");
            //     PerPrim_struct PerPrim[];
            builder.AppendLine($"\t{ssbo.StructName} {ssbo.InShaderName}[];");
            // };
            builder.AppendLine("};");
        }
    }

    private static void Ubos(StringBuilder builder, ReadOnlySpan<BufferDesc> ubos)
    {
        foreach (var ubo in ubos)
        {
            // layout(std140, binding = 0) uniform camera_struct {
            builder.AppendLine($"layout(std140, binding = {ubo.Binding}) uniform {ubo.StructName} {{");
            foreach (var field in ubo.StructFields)
            {
                //     mat4 View;
                builder.AppendLine($"    {ToGlName(field.Type)} {field.InShaderName};");
            }
            // } camera;
            builder.AppendLine($"}} {ubo.InShaderName};");
        }
    }
    
    public static string CreateVertex(    
        string userProcess,
        ReadOnlySpan<VaoAttribute> attributes,
        ReadOnlySpan<BufferDesc> ssbos,
        ReadOnlySpan<BufferDesc> ubos,
        StructDesc varyings)
    {
        var builder = new StringBuilder();
        // header
        builder.AppendLine("#version 430 core");
        
        Ssbos(builder, ssbos);
        Ubos(builder, ubos);
        VaryingsStruct(builder, varyings);
        FragmentInputStruct(builder, attributes);
        
        VertexIn(builder, attributes);
        VaryingsOut(builder, varyings);

        // user's process
        builder.AppendLine(userProcess);
        
        VertexMain(builder, varyings, attributes);
        
        return builder.ToString();
    }

    public static string CreateFragment(
        string userProcess,
        ReadOnlySpan<BufferDesc> ssbos,
        ReadOnlySpan<BufferDesc> ubos,
        StructDesc varyings)
    {
        var builder = new StringBuilder();
        // header
        builder.AppendLine("#version 430 core");

        Ssbos(builder, ssbos);
        Ubos(builder, ubos);
        VaryingsStruct(builder, varyings);
        
        VaryingsIn(builder, varyings);
        builder.AppendLine("out vec4 fragColor;");
        
        // user's process
        builder.AppendLine(userProcess);
        
        FragmentMain(builder, varyings);

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