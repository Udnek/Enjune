using Enjune.Graphic;

namespace Enjune.File.ModelReader;

public class ObjModelReader
{
    private readonly List<Position> _vertices = [];
    private readonly List<Mesh> _meshes = [];
    public readonly Mesh Mesh = null!;
    
    public ObjModelReader(ResourcePath path)
    {
        var text = FileManager.LoadText(path);
        var lines = text.Replace("\r", "").Split("\n");
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var error = ProcessLine(line.Split(' '));
            if (error != null)
            {
                Logger.Error($"can not parse line {i+1} \"{line}\" in file {path}: {error}");
                return;
            }
        }
        if (_meshes.Count == 0) Logger.Error("model is empty");
        Mesh = Mesh.MergeAll(_meshes);
    }

    private Position GetVertexById(int id)
    {
        if (id < 0) return _vertices[_vertices.Count + id];
        return _vertices[id-1]; // cause starts with 1
    }
    
    private string? Vertex(string[] args)
    {
        if (args.Length != 3) return "incorrect amount of args: " + args.Length;
        if (float.TryParse(args[0], out float x)
            && float.TryParse(args[1], out float y)
            && float.TryParse(args[2], out float z))
        {
            _vertices.Add(new Position(x, y, z));
            return null;
        }
        return "can not parse vertex coordinates";
    }

    private string? Face(string[] args)
    {
        if (args.Length < 3) return "amount of args must be at least 3:, but got " + args.Length;
        List<int> indexes = [];
        foreach (var arg in args)
        {
            var ver_tex_norm = arg.Split("/");
            if (ver_tex_norm.Length == 0) return "incorrect amount of values at arg: " + arg;
            if (int.TryParse(ver_tex_norm[0], out var index)) 
                indexes.Add(index);
            else
                return "can not parse arg: " + arg;
        }

        var poses = indexes.Select(GetVertexById).ToArray();
        if (poses.Length > 4)
            _meshes.Add(Mesh.Ngon(poses, TextureQuad.Furnace));
        else
            _meshes.Add(Mesh.Ngon(poses, TextureQuad.Tnt));

        return null;
    }
    
    private string? ProcessLine(string[] args)
    {
        if (args.Length == 0) return null;
        if (args[0].Length == 0) return null;
        return args.First() switch
        {
            "#" => null,
            "v" => Vertex(args.Skip(1).ToArray()),
            "f" => Face(args.Skip(1).ToArray()),
            _ => null
            // "mtllib" => null, // TODO implement
            // "usemtl" => null, // TODO implement
            // "g" => null, // TODO implement
            // "s" => null, // TODO implement
            // _ => "unknown sequence"
        };
    }
}