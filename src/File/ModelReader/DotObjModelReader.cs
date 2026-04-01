using Enjune.Graphic;
using Enjune.Misc;

namespace Enjune.File.ModelReader;

public class DotObjModelReader
{
    private readonly TextureManager _textureManager;
    private readonly ResourcePath _path;
    private readonly List<Position> _loadedVertices = [];
    private readonly List<TextureCoord> _loadedTextureCoords = [];
    private readonly List<Mesh> _meshes = [];
    private readonly Dictionary<string, DotObjMaterial> _materialByName = new();
    private DotObjMaterial? _selectedMaterial = null;
    private DotObjMaterial? _lastCreatedMaterial = null;

    public DotObjModelReader(TextureManager textureManager, ResourcePath path)
    {
        _textureManager = textureManager;
        _path = path;
    }

    public void Read(Consumer<Mesh> consumer, out string? error)
    {
        var text = FileManager.LoadText(_path, out error);
        if (text == null) return;
        var lines = text.Replace("\r", "").Split("\n");
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineError = ProcessLine(line.Split(' '));
            if (lineError != null)
            {
                Logger.Warn(this, $"error in line {i + 1} \"{line}\" in file {_path}: {lineError}");
            }
        }

        if (_meshes.Count == 0)
        {
            error = "model is empty";
            return;
        }
        Mesh.MergeThatHasSameTexture(_meshes, consumer);
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
            "vt" => VertexTexture(args.Skip(1).ToArray()),
            "mtllib" => MaterialLib(args.Skip(1).ToArray()),
            "usemtl" => UseMaterial(args.Skip(1).ToArray()),
            _ => null
        };
    }

    private string? VertexTexture(string[] args)
    {
        if (args.Length != 2) return "incorrect amount of args: " + args.Length;
        if (float.TryParse(args[0], out float u)
            && float.TryParse(args[1], out float v))
        {
            _loadedTextureCoords.Add(new TextureCoord(u, v));
            return null;
        }
        return "can not parse vertex textures";
    }

    private string? UseMaterial(string[] args)
    {
        if (args.Length == 0) return "not enough args";
        if (_materialByName.TryGetValue(args[0], out var mat))
        {
            _selectedMaterial = mat;
            return null;
        }
        return $"material is not defined: {args[0]}";
    }

    private string? MaterialLib(string[] args)
    {
        if (args.Length == 0) return "not enough args";
        var matPath = _path.ResolveFromLocal(args[0]);
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

    private string? NewMaterial(string[] args)
    {
        if (args.Length == 0) return "not enough args";
        var mat = new DotObjMaterial(args[0]);
        _materialByName[args[0]] = mat;
        _lastCreatedMaterial = mat;
        //Logger.Log($"adding material: {args[0]}");
        return null;
    }
    
    private string? CurrentMatTexture(string[] args)
    {
        if (_lastCreatedMaterial == null) return "material hasn't created";
        _lastCreatedMaterial.TexturePath = _path.ResolveFromLocal(args[0]);
        return null;
    }

    private string? ProcessMaterialLine(string[] args)
    {
        if (args.Length == 0) return null;
        if (args[0].Length == 0) return null;
        return args.First() switch
        {
            "#" => null,
            "newmtl" => NewMaterial(args.Skip(1).ToArray()),
            "map_Kd" => CurrentMatTexture(args.Skip(1).ToArray()),
            _ => null
        };
    }

    private T GetById<T>(List<T> from, int id)
    {
        if (id < 0) return from[from.Count + id];
        return from[id-1]; // cause starts with 1
    }
    
    private string? Vertex(string[] args)
    {
        if (args.Length != 3) return "incorrect amount of args: " + args.Length;
        if (float.TryParse(args[0], out float x)
            && float.TryParse(args[1], out float y)
            && float.TryParse(args[2], out float z))
        {
            _loadedVertices.Add(new Position(x, y, z));
            return null;
        }
        return "can not parse vertex coordinates";
    }

    private string? Face(string[] args)
    {
        if (args.Length < 3) return "amount of args must be at least 3:, but got " + args.Length;
        List<int> verIndexes = [];
        List<int> texIndexes = [];
        foreach (var arg in args)
        {
            var ver_tex_norm = arg.Split("/");
            if (ver_tex_norm.Length == 0) return "incorrect amount of values at arg: " + arg;
            if (int.TryParse(ver_tex_norm[0], out var verIndex)) 
                verIndexes.Add(verIndex);
            else
                return "can not parse vertex id: " + arg;

            if (ver_tex_norm.Length <= 1) continue;
            if (int.TryParse(ver_tex_norm[1], out var texIndex)) 
                texIndexes.Add(texIndex);
            // we do not return error here cause this arg can be omitted
        }

        var verPoses = verIndexes.Select(i => GetById(_loadedVertices, i)).ToArray();
        var texPoses = texIndexes.Select(i => GetById(_loadedTextureCoords, i)).ToArray();
        TexId textureId;
        if (_selectedMaterial?.TexturePath != null)
            textureId = _textureManager.AddTextureAndGetId(_selectedMaterial.TexturePath);
        else
            textureId = _textureManager.ErrorTexture;
        
        _meshes.Add(Mesh.Ngon(verPoses, texPoses, textureId));

        return null;
    }
}