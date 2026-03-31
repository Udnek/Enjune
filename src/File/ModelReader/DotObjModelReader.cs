using Enjune.Graphic;

namespace Enjune.File.ModelReader;

public class DotObjModelReader
{
    private readonly ResourcePath _path;
    private readonly List<Position> _vertices = [];
    private readonly List<Mesh> _meshes = [];
    private readonly Dictionary<string, DotObjMaterial> _materialByName = new();
    private DotObjMaterial? _lastMaterial = null;

    public DotObjModelReader(ResourcePath path)
    {
        _path = path;
    }

    public Mesh? Read(out string? error)
    {
        var text = FileManager.LoadText(_path, out error);
        if (text == null) return null;
        var lines = text.Replace("\r", "").Split("\n");
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineError = ProcessLine(line.Split(' '));
            if (lineError != null)
            {
                Logger.Warn($"skipping line {i + 1} \"{line}\" due error in file {_path}: {lineError}");
            }
        }

        if (_meshes.Count == 0)
        {
            error = "model is empty";
            return null;
        }
        return Mesh.MergeAll(_meshes);
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
            "mtllib" => MtlLib(args.Skip(1).ToArray()),
            _ => null
        };
    }

    private string? MtlLib(string[] args)
    {
        if (args.Length == 0) return "not enough args";
        var matPath = _path.Resolve(args[0]); // todo probably parse by '/'?
        var mat = FileManager.LoadText(matPath, out var error);
        if (mat == null) return error;
        var lines = mat.Replace("\r", "").Split("\n");
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            error = ProcessMaterialLine(line.Split(' '));
            if (error != null)
            {
                return $"can not parse line {i + 1} \"{line}\" in file {matPath}: {error}";
            }
        }
        return null;
    }

    private string? NewMlt(string[] args)
    {
        if (args.Length == 0) return null;
        var mat = new DotObjMaterial(args[1]);
        _materialByName[args[1]] = mat;
        _lastMaterial = mat;
        return null;
    }
    
    private string? CurrentMatTexture(string[] args)
    {
        if (_lastMaterial == null) return "selected material is null";
        _lastMaterial.TexturePath = _path.Resolve(args[0]);
        return null;
    }

    private string? ProcessMaterialLine(string[] args)
    {
        if (args.Length == 0) return null;
        if (args[0].Length == 0) return null;
        return args.First() switch
        {
            "#" => null,
            "newmtl" => NewMlt(args.Skip(1).ToArray()),
            "map_Kd" => CurrentMatTexture(args.Skip(1).ToArray()),
            _ => null
        };
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
            _meshes.Add(Mesh.Ngon(poses, TextureQuad.Planks));

        return null;
    }
}