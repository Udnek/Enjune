using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using SharpGLTF.Schema2;
using StbImageSharp;
using Mesh = Enjune.Graphic.Modeling.Mesh;


namespace Enjune.File.ModelReader;

public class DotGlbReader : AbstractModelReader
{
    protected override StaticModel? Read(out Error? error)
    {
        ModelRoot? gltfModel = null;
        Path.LoadStream(out error, stream =>
        {
            gltfModel = ModelRoot.ReadGLB(stream);
        });
        if (gltfModel == null) return null;

        var builder = new StaticModel.Builder();
        foreach (var mesh in gltfModel.LogicalMeshes)
        {
            
            foreach (var primitive in mesh.Primitives)
            {
                var primitiveType = primitive.DrawPrimitiveType;
                PrimitiveTopology? enjunePrimitive = primitiveType switch
                {
                    PrimitiveType.POINTS => PrimitiveTopology.Point,
                    PrimitiveType.LINES => PrimitiveTopology.Line,
                    PrimitiveType.LINE_LOOP => null,
                    PrimitiveType.LINE_STRIP => PrimitiveTopology.LineStrip,
                    PrimitiveType.TRIANGLES => PrimitiveTopology.Triangle,
                    PrimitiveType.TRIANGLE_STRIP => null,
                    PrimitiveType.TRIANGLE_FAN => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
                if (enjunePrimitive is null)
                {
                    Logger.Warn(this, $"{primitiveType} is not supported; skipping mesh");
                    continue;
                }
                
                var poses = primitive.GetVertexAccessor("POSITION").AsVector3Array().Select(v => v.ToTk()).ToArray();
                var texPoses = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array().Select(v => v.ToTk()).ToArray();
                var indices = primitive.GetIndices().Select(ui =>
                {
                    var i = (int)ui;
                    if (i >= 0) return i;
                    Logger.Warn(this, $"index overflows int: uint: {ui}; int: {i}");
                    return 0;
                }).ToArray();

                var material = primitive.Material;
                var compiledMat = GetMaterial(material);
                builder.Add(new MeshInstance.Entry
                {
                    Geometry = Mesh.Create(poses, texPoses, indices, enjunePrimitive.Value, true),
                    Material = compiledMat
                });
            }
        }

        return builder.Build(false);
    }

    private CompiledMaterial GetMaterial(Material material)
    {
        var channel = material.FindChannel("BaseColor");
        if (channel is null) return AssetManager.MissingMaterial;
        var rawMaterial = RawMaterial.White(material.Name);
        rawMaterial.LoadedTexture = Texture();
        rawMaterial.Color = channel.Value.Color.ToTk();

        return AssetManager.AddMaterialAndGetCompiled(rawMaterial);
        
        ByteImage? Texture()
        {
            var rawImg = channel.Value.Texture?.PrimaryImage;
            if (rawImg is null) return null;
            using var stream = rawImg.Content.Open();
            var img = ImageResult.FromStream(stream);
            return new ByteImage(img.Width, img.Height, ByteImage.Kind.FromStb(img.Comp), img.Data);
        }
    }
}








