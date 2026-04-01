using Enjune.Misc;

namespace Enjune.Graphic;

public class Mesh
{
    public readonly Position[] Vertices;
    public readonly int[] Indexes;
    public readonly TextureCoord[] Textures;
    public readonly TexId TextureId;

    public Mesh(Position[] vertices, TextureCoord[] textures, int[] indexes, int textureId)
    {
        if (!IsValid(vertices, textures, indexes, out var error)) 
            Logger.Error(this, "constructing incorrect mesh: " + error);
        
        Vertices = vertices;
        Textures = textures;
        Indexes = indexes;
        TextureId = textureId;
    }
    
    public static Mesh Cuboid(
        Position b1, Position b2, Position b3, Position b4,
        Position t1, Position t2, Position t3, Position t4,
        TextureQuad texture, TexId texId)
    {
        return MergeWithSameTexture(texId,
            Quad(b1, b2, b3, b4, texture, texId), // bot
            Quad(t1, t2, t3, t4, texture, texId), // top
            Quad(b1, b2, t2, t1, texture, texId), // front
            Quad(b2, b3, t3, t2, texture, texId), // right
            Quad(b3, b4, t4, t3, texture, texId), // back
            Quad(b4, b1, t1, t4, texture, texId)); // left
    }

    public static Mesh Cube(Position center, float size, TextureQuad texture, TexId texId)
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

            texture, texId
        );
    }

    public static Mesh Quad(Position bl, Position br, Position tr, Position tl, TextureQuad tex, TexId texId)
    {
        return new Mesh([bl, br, tr, tl], 
            [tex.BotLeft, tex.BotRight, tex.TopRight, tex.TopLeft], 
            [0, 1, 2, 0, 2, 3],
            texId
            );
    }
    
    public static Mesh Triangle(Position bl, Position br, Position tr, TextureQuad tex, TexId texId)
    {
        return new Mesh([bl, br, tr], 
            [tex.BotLeft, tex.BotRight, tex.TopRight], 
            [0, 1, 2],
            texId);
    }
    
    public string Mesh Ngon(Position[] poses, TextureCoord[] texCoords, TexId texId)
    {
        List<int> indexes = [];
        List<TextureCoord> textures = [];
        textures.Add(tex.BotLeft); // so it botLeft
        for (int i = 0; i < poses.Length-1; i++)
        {
            textures.Add(tex[i % 2 + 1]); // so it botRight or TopRight
        }
        for (int i = 0; i < poses.Length-1; i++)
        {
            // fan-like
            indexes.Add(0);
            indexes.Add(i);
            indexes.Add(i + 1);
        }
        return new Mesh(poses, textures.ToArray(), indexes.ToArray(), texId);
    }
    
    public static Mesh Ngon(Position[] poses, TextureQuad tex, TexId texId)
    {
        if (poses.Length == 3) return Triangle(poses[0], poses[1], poses[2], tex, texId);
        if (poses.Length == 4) return Quad(poses[0], poses[1], poses[2], poses[3], tex, texId);
        List<int> indexes = [];
        List<TextureCoord> textures = [];
        textures.Add(tex.BotLeft); // so it botLeft
        for (int i = 0; i < poses.Length-1; i++)
        {
            textures.Add(tex[i % 2 + 1]); // so it botRight or TopRight
        }
        for (int i = 0; i < poses.Length-1; i++) 
        {
            // fan-like
            indexes.Add(0);
            indexes.Add(i);
            indexes.Add(i + 1);
        }
        return new Mesh(poses, textures.ToArray(), indexes.ToArray(), texId);
    }


    public static void MergeThatHasSameTexture(IEnumerable<Mesh> meshes, Consumer<Mesh> mergedMeshesConsumer)
    {
        foreach (var groupedByTex in meshes.GroupBy(m => m.TextureId))
        {
            var texId = groupedByTex.Key;
            mergedMeshesConsumer(MergeWithSameTexture(texId, groupedByTex));
        }
    }
    
    public static Mesh MergeWithSameTexture(TexId texId, params Mesh[] meshes) 
        => MergeWithSameTexture(texId, (IEnumerable<Mesh>) meshes);
    
    public static Mesh MergeWithSameTexture(TexId texId, IEnumerable<Mesh> meshes)
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
        return new Mesh(vertices, textures, indexes.ToArray(), texId);
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