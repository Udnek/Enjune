using Enjune.Misc;

namespace Enjune.Graphic;

public class Mesh
{
    public readonly Position[] Vertices;
    public readonly int[] Indexes;
    public readonly TextureCoord[] Textures;
    public readonly MatId MatId;

    public Mesh(Position[] vertices, TextureCoord[] textures, int[] indexes, int matId)
    {
        if (!IsValid(vertices, textures, indexes, out var error)) 
            Logger.Error(this, "constructing incorrect mesh: " + error);
        
        Vertices = vertices;
        Textures = textures;
        Indexes = indexes;
        MatId = matId;
    }
    
    public static Mesh Cuboid(
        Position b1, Position b2, Position b3, Position b4,
        Position t1, Position t2, Position t3, Position t4,
        TextureQuad texture, MatId matId)
    {
        return MergeAndSetMaterial(matId,
            Quad(b1, b2, b3, b4, texture, matId), // bot
            Quad(t1, t2, t3, t4, texture, matId), // top
            Quad(b1, b2, t2, t1, texture, matId), // front
            Quad(b2, b3, t3, t2, texture, matId), // right
            Quad(b3, b4, t4, t3, texture, matId), // back
            Quad(b4, b1, t1, t4, texture, matId)); // left
    }

    public static Mesh Cube(Position center, float size, TextureQuad texture, MatId matId)
    {
        var hs = size / 2;
        return Cuboid(
            // bottom
            center + (-hs, -hs, -hs), //-x -z
            center + (-hs, -hs, +hs), //+x -z
            center + (+hs, -hs, +hs), //+x +z
            center + (+hs, -hs, -hs), //-x +z
            // top
            center + (-hs, hs, -hs), //-x -z
            center + (-hs, hs, +hs), //+x -z
            center + (+hs, hs, +hs), //+x +z
            center + (+hs, hs, -hs), //-x +z

            texture, matId
        );
    }

    public static Mesh Quad(Position bl, Position br, Position tr, Position tl, TextureQuad tex, MatId matId)
    {
        return new Mesh([bl, br, tr, tl], 
            [tex.BotLeft, tex.BotRight, tex.TopRight, tex.TopLeft], 
            [0, 1, 2, 0, 2, 3],
            matId
            );
    }
    
    public static Mesh Triangle(Position bl, Position br, Position tr, TextureQuad tex, MatId matId)
    {
        return new Mesh([bl, br, tr], 
            [tex.BotLeft, tex.BotRight, tex.TopRight], 
            [0, 1, 2],
            matId);
    }
    
    public static Mesh Ngon(Position[] poses, TextureCoord[] texCoords, MatId matId)
    {
        if (texCoords.Length != poses.Length)
            throw new ArgumentException($"positions and text coordinates must have the same length: {poses.Length} != {texCoords.Length}");
        List<int> indexes = [];
        for (int i = 1; i < poses.Length-1; i++)
        {
            // fan-like
            indexes.Add(0);
            indexes.Add(i);
            indexes.Add(i + 1);
        }
        return new Mesh(poses, texCoords.ToArray(), indexes.ToArray(), matId);
    }
    
    public static void MergeThatHaveSameMaterial(IEnumerable<Mesh> meshes, Consumer<Mesh> mergedMeshesConsumer)
    {
        foreach (var groupedByMat in meshes.GroupBy(m => m.MatId))
        {
            var matId = groupedByMat.Key;
            mergedMeshesConsumer(MergeAndSetMaterial(matId, groupedByMat));
        }
    }
    
    public static Mesh MergeAndSetMaterial(MatId matId, params Mesh[] meshes) 
        => MergeAndSetMaterial(matId, (IEnumerable<Mesh>) meshes);
    
    public static Mesh MergeAndSetMaterial(MatId matId, IEnumerable<Mesh> meshes)
    {
        int offset = 0;
        var indexes = new List<int>();
        foreach (var mesh in meshes)
        {
            foreach (var index in mesh.Indexes) indexes.Add(index + offset);
            offset += mesh.Vertices.Length;
        }
        var textures = meshes.SelectMany(mesh => mesh.Textures).ToArray();
        var vertices = meshes.SelectMany(mesh => mesh.Vertices).ToArray();
        return new Mesh(vertices, textures, indexes.ToArray(), matId);
    }

    public static bool IsValid(Position[] vertices, TextureCoord[] textures, int[] indexes, out string? error)
    {
        if (vertices.Length < 3)
        {
            error = $"vertices array < 3: mesh doesn't really make sense";
            return false;
        }
        if (vertices.Length != textures.Length)
        {
            error = $"vertices and texture sizes aren't equal: {vertices.Length} != {textures.Length}";
            return false;
        }
        var min = indexes.Min(i => i);
        if (min != 0)
        {
            error = $"index must be > 0, but got: {min}";
            return false;
        }
        var max = indexes.Max(i => i);
        if (max >= vertices.Length)
        {
            error = $"index must be < lenght of vertices ({vertices.Length}), but got: {max}";
            return false;
        }
        for (int i=0; i<vertices.Length; i++)
        {
            if (indexes.Contains(i)) continue;
            error = $"unused vertex at: {i}";
            return false;
        }
        
        error = null;
        return true;
    }
}