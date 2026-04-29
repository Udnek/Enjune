using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using Mesh = Enjune.Graphic.Mesh;


namespace Enjune.File.ModelReader;

public class DotGlbReader : AbstractReader
{
    public DotGlbReader(AssetManager assetManager, ResourcePath path) : base(assetManager, path) { }

    public override Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>? Read(out Error? error)
    {
        ModelRoot? gltfModel = null;
        Path.LoadStream(out error, stream =>
        {
            gltfModel = ModelRoot.ReadGLB(stream);
        });
        if (gltfModel == null) return null;

        var builder = new Model<(TextureCoord, Vector3), CompiledMaterial>.Builder();
        foreach (var mesh in gltfModel.LogicalMeshes)
        {
            foreach (var primitive in mesh.Primitives)
            {
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
                builder.Add(Mesh.CreateWithNormals(poses, texPoses, indices), compiledMat);
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
            var img = StbImageSharp.ImageResult.FromStream(stream);
            return new ByteImage(img.Width, img.Height, ByteImage.ImType.FromStb(img.Comp), img.Data);
        }
    }
}








