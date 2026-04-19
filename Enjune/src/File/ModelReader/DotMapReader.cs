using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;

namespace Enjune.File.ModelReader;

public class DotMapReader
{
    private readonly AssetManager _assetManager;
    private readonly ResourcePath _path;

    public DotMapReader(AssetManager assetManager, ResourcePath path)
    {
        _assetManager = assetManager;
        _path = path;
    }

    public Model<TextureCoord, CompiledMaterial>? Read(out Error? error)
    {
        var mapFormat = new QuakeMapFormat();
        MapFile? mapFile = null;
        _path.LoadStream(out error, s => { mapFile = mapFormat.Read(s); });
        if (mapFile == null)
        {
            error = "can not load map: " + error;
            return null;
        }
        return ProceedMap(mapFile);
    }

    private Model<TextureCoord, CompiledMaterial> ProceedMap(MapFile map)
    {
        var builder = new Model<TextureCoord, CompiledMaterial>.Builder();
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
                var material = _assetManager.AddMaterialAndGetCompiled(RawMaterial.FromTexture(_path.ResolveRaw(face.TextureName)));
                builder.Add(Mesh<TextureCoord>.Ngon(positions, texCoords), material);
            }

            // foreach (var mesh in solid.Meshes)
            // {
            //     var vertices = new Position[mesh.Points.Count];
            //     var texCoords = new TextureCoord[mesh.Points.Count];
            //     for (var i = 0; i < mesh.Points.Count; i++)
            //     {
            //         var meshPoint = mesh.Points[i];
            //         vertices[i] = new Position(meshPoint.Position.X, meshPoint.Position.Y, meshPoint.Position.Z);
            //         texCoords[i] = new TextureCoord(meshPoint.Texture.X, meshPoint.Texture.Y);
            //     }
            //     var material = _assetManager.AddMaterialAndGetCompiled(RawMaterial.FromTexture(_path.ResolveRaw(mesh.TextureName)));
            //     builder.Add(Mesh<TextureCoord>.Ngon(vertices, texCoords), material);
            // }
        }

        return builder.Build();
    }
}