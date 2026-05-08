using Enjune.Misc;

namespace Enjune.Graphic;

public class Mesh : AbstractMesh<Mesh.PerVertex>
{
    public record struct PerVertex(Vector2 TexPos, Vector3 Normal);

    public Mesh(Position[] vertices, PerVertex[] perVertexData, int[] indexes) : base(vertices, perVertexData, indexes){}
    
    // STATIC
    
      public static Mesh CreateWithNormals(
        Position[] vertices, TexturePos[] texPos, int[] indexes)
    {
        var normals = GenerateSmoothNormals(vertices, indexes);
        return new Mesh(vertices, 
            texPos.Select((tc, i) => new PerVertex(tc, normals[i])).ToArray(), 
            indexes);
    }

    public static Mesh Cuboid(
        Position b1, Position b2, Position b3, Position b4,
        Position t1, Position t2, Position t3, Position t4,
        TextureQuad texture)
    {
        return Merge(
            Quad(b1, b2, b3, b4, texture), // bot
            Quad(t4, t3, t2, t1, texture), // top
            Quad(t1, t2, b2, b1, texture), // front
            Quad(t2, t3, b3, b2, texture), // right
            Quad(t3, t4, b4, b3, texture), // back
            Quad(t4, t1, b1, b4, texture)); // left
    }

    public static Mesh Cube(Position center, float size, TextureQuad texture)
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

    public static Mesh Quad(Position bl, Position br, Position tr, Position tl,
        TextureQuad tex)
    {
        return CreateWithNormals([bl, br, tr, tl],
            [tex.BotLeft, tex.BotRight, tex.TopRight, tex.TopLeft],
            [0, 1, 2, 0, 2, 3]);
    }


    public static Mesh Triangle<TPerVert>(Position p1, Position p2, Position p3, 
        Mesh.PerVertex pv1, Mesh.PerVertex pv2, Mesh.PerVertex pv3)
    {
        return new Mesh([p1, p2, p3],
            [pv1, pv2, pv3],
            [0, 1, 2]);
    }


    public static Mesh Triangle(Position bl, Position br, Position tr,
        TextureQuad tex)
    {
        return CreateWithNormals([bl, br, tr],
            [tex.BotLeft, tex.BotRight, tex.TopRight],
            [0, 1, 2]);
    }


    public static Mesh Ngon(Position[] poses, Mesh.PerVertex[] perVertexData)
    {
        if (perVertexData.Length != poses.Length)
            throw new ArgumentException(
                $"positions and perVertexData must have the same length: {poses.Length} != {perVertexData.Length}");
        List<int> indexes = new(poses.Length * 3);
        for (var i = 1; i < poses.Length - 1; i++)
        {
            // fan-like
            indexes.Add(0);
            indexes.Add(i);
            indexes.Add(i + 1);
        }

        return new Mesh(poses, perVertexData, indexes.ToArray());
    }




    public static Mesh NgonWithNormals(Position[] poses, TexturePos[] texPoses)
    {
        if (texPoses.Length != poses.Length)
            throw new ArgumentException(
                $"positions and texPoses must have the same length: {poses.Length} != {texPoses.Length}");
        List<int> indexes = new(poses.Length * 3);
        for (var i = 1; i < poses.Length - 1; i++)
        {
            // fan-like
            indexes.Add(0);
            indexes.Add(i);
            indexes.Add(i + 1);
        }

        var arrayIndexes = indexes.ToArray();
        var normals = GenerateSmoothNormals(poses, arrayIndexes);
        return new Mesh(poses, 
            texPoses.Select((tc, i) => new Mesh.PerVertex(tc, normals[i])).ToArray(), 
            arrayIndexes);
    }

    public static Vector3[] GenerateSmoothNormals(Position[] vertices, int[] indexes)
    {
        if (vertices.Length <= 2)
        {
            Logger.Error(typeof(AbstractMesh<object>), "trying to generate smooth normals for < 3 verices");
            return Enumerable.Repeat(Vector3.UnitX, vertices.Length).ToArray();
        }

        if (indexes.Length % 3 != 0)
        {
            Logger.Error(typeof(AbstractMesh<object>), "trying to generate smooth normals for indexes length % 3 != 0");
            return Enumerable.Repeat(Vector3.UnitX, vertices.Length).ToArray();
        }

        Vector3[] normals = new Vector3[vertices.Length];
        for (int iIndex = 0; iIndex < indexes.Length; iIndex += 3)
        {
            var v0Idx = indexes[iIndex];
            var v1Idx = indexes[iIndex + 1];
            var v2Idx = indexes[iIndex + 2];

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
    
    public static Mesh Merge(params Mesh[] meshes)
        => Merge((IEnumerable<Mesh>) meshes);

    public static Mesh Merge(IEnumerable<Mesh> meshes)
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
        return new Mesh(vertices, perVertexData, indexes.ToArray());
    }
}