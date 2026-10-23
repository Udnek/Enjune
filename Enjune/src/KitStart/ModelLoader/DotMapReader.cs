using Enjune.Graphic.Modeling;
using Enjune.KitStart.Modeling;
using Enjune.KitStart.TAAL;
using Enjune.Misc;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;

namespace Enjune.KitStart.ModelLoader;

public class DotMapReader : AbstractModelReader
{
    protected override StaticModel<TAALData.Vertex, TAALData.PerPrimitive>? Read(out Error? error)
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

    private StaticModel<TAALData.Vertex, TAALData.PerPrimitive> ProceedMap(MapFile map)
    {
        var builder = new StaticModel<TAALData.Vertex, TAALData.PerPrimitive>.Builder();
        var solids = map.Worldspawn.Find(mo => mo is Solid).Cast<Solid>();
        
        foreach (var solid in solids)
        {
            foreach (var face in solid.Faces)
            {
                var positions = new Position[face.Vertices.Count];
                var texCoords = new TexturePos[face.Vertices.Count];
                for (var i = 0; i < face.Vertices.Count; i++)
                {
                    var faceVertex = face.Vertices[i];
                    var pos = faceVertex.ToTk();
                    positions[i] = pos;
                    var u = (Vector3.Dot(pos, face.UAxis.ToTk()) + face.XShift) / face.XScale;
                    var v = (Vector3.Dot(pos, face.VAxis.ToTk()) + face.YShift) / face.YScale;
                    Logger.Info(this, $"u: {u}; v: {v}");
                    texCoords[i] = (u, v);
                }
                var basicVertices = new TAALData.Vertex[positions.Length];
                for (var i = 0; i < positions.Length; i++)
                {
                    basicVertices[i] = new TAALData.Vertex()
                    {
                        Position = positions[0],
                        Uv = texCoords[0]
                    };
                }
                
                var matIdx = TextureArrayAssetLoader.AddMaterialAndGetIndex(
                    RawTAALMaterial.FromTexture(Path.ResolveRaw(face.TextureName)));
                
                builder.GetMeshWithTopology(PrimitiveTopology.Triangle)
                    .AddNgon(new TAALData.PerPrimitive(matIdx), basicVertices);
            }
        }

        return builder.Build();
    }
}