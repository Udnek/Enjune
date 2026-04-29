using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;
using Mesh = Enjune.Graphic.Mesh;

namespace Enjune.File.ModelReader;

public class DotMapReader : AbstractReader
{
    public DotMapReader(AssetManager assetManager, ResourcePath path) : base(assetManager, path){}
    
    public override MaterialModel? Read(out Error? error)
    {
        var mapFormat = new QuakeMapFormat();
        MapFile? mapFile = null;
        Path.LoadStream(out error, s => { mapFile = mapFormat.Read(s); });
        if (mapFile == null)
        {
            error = "can not load map: " + error;
            return null;
        }
        return ProceedMap(mapFile);
    }

    private MaterialModel ProceedMap(MapFile map)
    {
        var builder = new MaterialModel.Builder();
        List<Mesh<Color>> meshes = [];
        var solids = map.Worldspawn.Find(mo => mo is Solid).Cast<Solid>();
        
        foreach (var solid in solids)
        {
            foreach (var face in solid.Faces)
            {
                var positions = new Position[face.Vertices.Count];
                var texCoords = new TextureCoord[face.Vertices.Count];
                for (var i = 0; i < face.Vertices.Count; i++)
                {
                    var faceVertex = face.Vertices[i];
                    var pos = faceVertex.ToTk();
                    positions[i] = pos;
                    float u = (Vector3.Dot(pos, face.UAxis.ToTk()) + face.XShift) / face.XScale;
                    float v = (Vector3.Dot(pos, face.VAxis.ToTk()) + face.YShift) / face.YScale;
                    Logger.Log(this, $"u: {u}; v: {v}");
                    texCoords[i] = (u, v);
                }
                var material = AssetManager.AddMaterialAndGetCompiled(RawMaterial.FromTexture(Path.ResolveRaw(face.TextureName)));
                builder.Add(Mesh.NgonWithNormals(positions, texCoords), material);
            }
        }

        return builder.Build();
    }
}