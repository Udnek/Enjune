using Enjune.Misc;

namespace Enjune.Graphic;

public sealed class Mesh<TPerVertex>
{
    public readonly Position[] Vertices;
    public readonly int[] Indexes;
    public readonly TPerVertex[] PerVertexData;

    public Mesh(Position[] vertices, TPerVertex[] perVertexData, int[] indexes)
    {
        if (!IsValid(vertices, perVertexData, indexes, out var error)) 
            Logger.Error(this, "constructing invalid mesh: " + error);
        
        Vertices = vertices;
        PerVertexData = perVertexData;
        Indexes = indexes;
    }
    
    public void Offset(Position offset)
    {
        for (var i = 0; i < Vertices.Length; i++) 
            Vertices[i] += offset;
    }

    public static Mesh<(TextureCoord texCoord, Normal normal)> CreateWithNormals(
        Position[] vertices, TextureCoord[] texCoords, int[] indexes)
    {
        var normals = GenerateSmoothNormals(vertices, indexes);
        return new Mesh<(TextureCoord texCoord, Normal normal)>
            (vertices, texCoords.JoinToTuple(normals), indexes);
    }
    
    public static Mesh<(TextureCoord texCoord, Normal normal)> Cuboid(
        Position b1, Position b2, Position b3, Position b4,
        Position t1, Position t2, Position t3, Position t4,
        TextureQuad texture)
    {
        return Mesh<(TextureCoord texCoord, Normal normal)>.Merge(
            Quad(b1, b2, b3, b4, texture), // bot
            Quad(t4, t3, t2, t1, texture), // top
            Quad(t1, t2, b2, b1, texture), // front
            Quad(t2, t3, b3, b2, texture), // right
            Quad(t3, t4, b4, b3, texture), // back
            Quad(t4, t1, b1, b4, texture)); // left
    }

    public static Mesh<(TextureCoord texCoord, Normal normal)> Cube(Position center, float size, TextureQuad texture)
    {
        var hs = size / 2;
        return Cuboid(
            // bottom
            center + (-hs, -hs, -hs), //-x -z
            center + (+hs, -hs, -hs), //+x -z
            center + (+hs, -hs, +hs), //+x +z
            center + (-hs, -hs, +hs), //-x +z
            // top
            center + (-hs, +hs, -hs), //-x -z
            center + (+hs, +hs, -hs), //+x -z
            center + (+hs, +hs, +hs), //+x +z
            center + (-hs, +hs, +hs), //-x +z

            texture
        );
    }

    public static Mesh<(TextureCoord texCoord, Normal normal)> Quad(Position bl, Position br, Position tr, Position tl, TextureQuad tex)
    {
        return CreateWithNormals([bl, br, tr, tl],
                [tex.BotLeft, tex.BotRight, tex.TopRight, tex.TopLeft],
                [0, 1, 2, 0, 2, 3]);
    }
    
    public static Mesh<(TextureCoord texCoord, Normal normal)> Triangle(Position bl, Position br, Position tr, TextureQuad tex)
    {
        return CreateWithNormals([bl, br, tr],
            [tex.BotLeft, tex.BotRight, tex.TopRight],
            [0, 1, 2]);
    }
    
    
    public static Mesh<(TPerVertex, Normal)> NgonWithNormals(Position[] poses, TPerVertex[] perVertexData)
    {
        if (perVertexData.Length != poses.Length)
            throw new ArgumentException($"positions and perVertexData must have the same length: {poses.Length} != {perVertexData.Length}");
        List<int> indexes = new (poses.Length*3);
        for (int i = 1; i < poses.Length-1; i++)
        {
            // fan-like
            indexes.Add(0);
            indexes.Add(i);
            indexes.Add(i + 1);
        }

        var arrayIndexes = indexes.ToArray();
        var normals = GenerateSmoothNormals(poses, arrayIndexes);
        return new Mesh<(TPerVertex, Vector3)>(poses, perVertexData.JoinToTuple(normals), arrayIndexes);
    }
    public static Mesh<TPerVertex> Ngon(Position[] poses, TPerVertex[] perVertexData)
    {
        if (perVertexData.Length != poses.Length)
            throw new ArgumentException($"positions and perVertexData must have the same length: {poses.Length} != {perVertexData.Length}");
        List<int> indexes = new (poses.Length*3);
        for (int i = 1; i < poses.Length-1; i++)
        {
            // fan-like
            indexes.Add(0);
            indexes.Add(i);
            indexes.Add(i + 1);
        }
        return new Mesh<TPerVertex>(poses, perVertexData, indexes.ToArray());
    }

    public static Vector3[] GenerateSmoothNormals(Position[] vertices, int[] indexes)
    {
        if (vertices.Length <= 2)
        {
            Logger.Error(typeof(Mesh<object>), "trying to generate smooth normals for < 3 verices");
            return Enumerable.Repeat(Vector3.UnitX, vertices.Length).ToArray();
        }

        if (indexes.Length % 3 != 0)
        {
            Logger.Error(typeof(Mesh<object>), "trying to generate smooth normals for indexes length % 3 != 0");
            return Enumerable.Repeat(Vector3.UnitX, vertices.Length).ToArray();
        }
        Vector3[] normals = new Vector3[vertices.Length];
        for (int iIndex = 0; iIndex < indexes.Length; iIndex+=3)
        {
            var v0Idx = indexes[iIndex];
            var v1Idx = indexes[iIndex+1];
            var v2Idx = indexes[iIndex+2];
            
            // we do not normalize, cause final norm will be impacted by square of triangle
            var norm = MathUtils.PlaneNormNotNormalized(
                vertices[v0Idx], vertices[v1Idx], vertices[v2Idx]);
            normals[v0Idx] += norm;
            normals[v1Idx] += norm;
            normals[v2Idx] += norm;
        }
        for (var i = 0; i < normals.Length; i++) 
            normals[i] = normals[i].Normalized();
        
        return normals;
    }
    
    public static Mesh<TPerVertex> Merge(params Mesh<TPerVertex>[] meshes) => Merge((IEnumerable<Mesh<TPerVertex>>) meshes);
    
    public static Mesh<TPerVertex> Merge(IEnumerable<Mesh<TPerVertex>> meshes)
    {
        int offset = 0;
        var indexes = new List<int>();
        foreach (var mesh in meshes)
        {
            foreach (var index in mesh.Indexes) 
                indexes.Add(offset + index);
            offset += mesh.Vertices.Length;
        }
        var perVertexData = meshes.SelectMany(mesh => mesh.PerVertexData).ToArray();
        var vertices = meshes.SelectMany(mesh => mesh.Vertices).ToArray();
        return new Mesh<TPerVertex>(vertices, perVertexData, indexes.ToArray());
    }

    public static bool IsValid(Position[] vertices, TPerVertex[] perVertexData, int[] indexes, out Error? error)
    {
        if (vertices.Length <= 1)
        {
            error = "vertices array <= 1: mesh doesn't really make sense";
            return false;
        }
        if (vertices.Length != perVertexData.Length)
        {
            error = $"vertices and perVertexData sizes aren't equal: {vertices.Length} != {perVertexData.Length}";
            return false;
        }
        var min = indexes.Min();
        if (min != 0)
        {
            error = $"index must be > 0, but got: {min}";
            return false;
        }
        var max = indexes.Max();
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