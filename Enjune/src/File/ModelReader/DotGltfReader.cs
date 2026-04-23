using Enjune.Graphic;
using Enjune.Graphic.Asset;
using SharpGLTF.Schema2;


namespace Enjune.File.ModelReader;

public class DotGltfReader : AbstractReader
{
    public DotGltfReader(AssetManager assetManager, ResourcePath path) : base(assetManager, path) { }

    public override Model<TextureCoord, CompiledMaterial>? Read(out Error? error)
    {
        Gltf? model = null;
        Path.LoadStream(out error, stream =>
        {
            model = ModelRoot.ParseGLB(stream);
        });
        if (model == null) return null;
        
       foreach (var mesh in model.Meshes)
       {
           foreach (var primitive in mesh.Primitives)
           {
               var primitiveIndices = primitive.Indices;
           }
       }
        
    }
}