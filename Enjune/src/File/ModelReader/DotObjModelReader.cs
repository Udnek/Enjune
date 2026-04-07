using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using Buffer = System.Buffer;

namespace Enjune.File.ModelReader;

public class DotObjModelReader
{
    private readonly AssetManager _assetManager;
    private readonly ResourcePath _path;
    private readonly List<Position> _loadedVertices = [];
    private readonly List<TextureCoord> _loadedTextureCoords = [];
    private readonly Dictionary<string, RawMaterial> _materialByName = new();
    private RawMaterial? _selectedMaterial = null;
    private RawMaterial? _lastCreatedMaterial = null;  
    
    private readonly Model.Builder _builder = new();

    public DotObjModelReader(AssetManager assetManager, ResourcePath path)
    {
        _assetManager = assetManager;
        _path = path;
    }

    public Model? Read(out string? error)
    {
        var text = _path.LoadText(out error);
        if (text == null) return null;
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

        if (!_builder.IsEmpty) return _builder.Build();
        
        error = "model is empty";
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
        var matPath = _path.ResolveRaw(string.Join(" ", args));
        var mat = matPath.LoadText(out var error);
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
        var name = string.Join(" ", args);
        var mat = RawMaterial.FromColor(new Color(0.8f, 0.8f, 0.8f, 1f)); // default color in .obj
        _materialByName[name] = mat;
        _lastCreatedMaterial = mat;
        return null;
    }
    
    private string? CurrentMatTexture(string[] args)
    {
        if (args.Length == 0) return "not enough args";
        if (_lastCreatedMaterial == null) return "material hasn't created";
        _lastCreatedMaterial.TexturePath = _path.ResolveRaw(string.Join(" ", args));
        return null;
    }
    
    private string? CurrentMatColor(string[] args)
    {
        if (_lastCreatedMaterial == null) return "material hasn't created";
        if (float.TryParse(args[0], out float r)
            && float.TryParse(args[1], out float g)
            && float.TryParse(args[2], out float b))
        {
            _lastCreatedMaterial.Color = new Color(r, g, b, 1);
            return null;
        }
        return "can not parse rgb";
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
            "Kd" => CurrentMatColor(args.Skip(1).ToArray()),
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
        TextureCoord[] texPoses;
        if (texIndexes.Count == 0) // then we filling it by ourselves 
        {
            var texPosesList = new List<TextureCoord>();
            for (var i = 0; i < verIndexes.Count; i++)
            {
                texPosesList.Add(TextureQuad.Full[i % 4]);
            }
            texPoses = texPosesList.ToArray();
        }
        else
            texPoses = texIndexes.Select(i => GetById(_loadedTextureCoords, i)).ToArray();
        
        CompiledMaterial material;
        if (_selectedMaterial != null)
            material = _assetManager.AddMaterialAndGetCompiled(_selectedMaterial);
        else
            material = _assetManager.MissingMaterial;

        _builder.Add(Mesh.Ngon(verPoses, texPoses), material);
        return null;
    }
}