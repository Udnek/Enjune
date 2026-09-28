using System.Diagnostics.Contracts;
using Enjune.Attribute;
using Enjune.Misc;

namespace Enjune.Graphic.Modeling;

/// <summary>
/// Only geometry
/// </summary>
[LogParams(logCallingMethod: true)]
public sealed class Mesh
{
    public readonly PrimitiveTopology Topology;
    public readonly Position[] Vertices;
    public readonly int[] Indexes;
    public readonly PerVertex[] PerVertexData;
    
    public record struct PerVertex(Vector2 TexPos, Vector3 Normal);
    
    public Mesh(Position[] vertices, PerVertex[] perVertexData, int[] indexes, PrimitiveTopology topology)
    {
        if (!IsValid(topology, vertices, perVertexData, indexes, out var error)) 
            Logger.Error(this, "constructing invalid mesh: " + error);

        Topology = topology;
        Vertices = vertices;
        PerVertexData = perVertexData;
        Indexes = indexes;
    }
    
    public void Move(Position offset)
    {
        for (var i = 0; i < Vertices.Length; i++) 
            Vertices[i] += offset;
    }

    public void Scale(Vector3 factor)
    {
        for (int i = 0; i < Vertices.Length; i++) 
            Vertices[i] *= factor;
    }

    public (Vector3 Min, Vector3 Max) Bounds()
    {
        if (Vertices.Length == 0)
            return (Vector3.Zero, Vector3.Zero);
        
        var min = Vertices[0];
        var max = Vertices[0];
        for (var i = 1; i < Vertices.Length; i++)
        {
            var vertex = Vertices[i];
            max = Vector3.ComponentMax(max, vertex);
            min = Vector3.ComponentMin(min, vertex);
        }

        return (min, max);
    }
    
    // STATIC

    public static Mesh Empty(PrimitiveTopology topology) => new([], [], [], topology);

    public static Mesh Create(
        Position[] vertices, TexturePos[] texPos, int[] indexes, PrimitiveTopology topology = PrimitiveTopology.Triangle, bool calculateNormals = true)
    {
        // normals only works with triangles
        if (calculateNormals && topology == PrimitiveTopology.Triangle)
        {
            var normals = GenerateSmoothNormals(vertices, indexes);
            return new Mesh(vertices, 
                texPos.Select((tc, i) => new PerVertex(tc, normals[i])).ToArray(), 
                indexes,
                topology);
        }
        return new Mesh(vertices, 
            texPos.Select(tc => new PerVertex(tc, Vector3.UnitX)).ToArray(), 
            indexes,
            topology);
    }

    public static Mesh Cuboid(
        Position b1, Position b2, Position b3, Position b4,
        Position t1, Position t2, Position t3, Position t4,
        TextureQuad texture, PrimitiveTopology topology, bool calculateNormals)
    {
        return Merge(
            Quad(b1, b2, b3, b4, texture, topology, calculateNormals), // bot
            Quad(t4, t3, t2, t1, texture, topology, calculateNormals), // top
            Quad(t1, t2, b2, b1, texture, topology, calculateNormals), // front
            Quad(t2, t3, b3, b2, texture, topology, calculateNormals), // right
            Quad(t3, t4, b4, b3, texture, topology, calculateNormals), // back
            Quad(t4, t1, b1, b4, texture, topology, calculateNormals)); // left
    }

    public static Mesh Cube(Position center, float size, TextureQuad texture, PrimitiveTopology topology = PrimitiveTopology.Triangle, bool calculateNormals = true)
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

            texture, topology, calculateNormals
        );
    }

    public static Mesh Quad(Position bl, Position br, Position tr, Position tl,
        TextureQuad tex, PrimitiveTopology topology = PrimitiveTopology.Triangle, bool calculateNormals = true)
    {
        return Create([bl, br, tr, tl],
            [tex.BotLeft, tex.BotRight, tex.TopRight, tex.TopLeft],
            [0, 1, 2, 0, 2, 3], topology, calculateNormals);
    }

    public static Mesh QuadXy(Position bl, float width, float height, 
        TextureQuad tex, PrimitiveTopology topology = PrimitiveTopology.Triangle, bool calculateNormals = true)
    {
        return Quad(bl, bl + (width, 0, 0), bl + (width, height, 0), bl + (0, height, 0), 
            TextureQuad.Full, topology, calculateNormals);
    }
    
    public static Mesh Triangle(Position bl, Position br, Position tr,
        TextureQuad tex, PrimitiveTopology topology = PrimitiveTopology.Triangle, bool calculateNormals = true)
    {
        return Ngon([bl, br, tr], [tex.BotLeft, tex.BotRight, tex.TopRight], topology, calculateNormals);
    }
    
    public static Mesh Ngon(Position[] poses, TexturePos[] texPoses, 
        PrimitiveTopology topology = PrimitiveTopology.Triangle, bool calculateNormals = true)
    {
        if (texPoses.Length != poses.Length)
            throw new ArgumentException(
                $"{nameof(poses)} and {nameof(texPoses)} must have the same length: {poses.Length} != {texPoses.Length}");

        if (topology == PrimitiveTopology.Triangle)
        {
            List<int> indexes = new(poses.Length * 3);
            for (var i = 1; i < poses.Length - 1; i++)
            {
                // fan-like
                indexes.Add(0);
                indexes.Add(i);
                indexes.Add(i + 1);
            }

            return Create(poses, texPoses, indexes.ToArray(), topology, calculateNormals); 
        }
        
        if (topology == PrimitiveTopology.Line)
        {
            List<int> indexes = new(poses.Length * 2);
            for (var i = 0; i < poses.Length; i++)
            {
                indexes.Add(i);
                indexes.Add((i + 1) % poses.Length);
            }

            return Create(poses, texPoses, indexes.ToArray(), topology, calculateNormals); 
        }
        
        if (topology == PrimitiveTopology.LineStrip)
        {
            List<int> indexes = new(poses.Length+1);
            for (var i = 0; i <= poses.Length; i++)
            {
                indexes.Add(i % poses.Length);
            }
            return Create(poses, texPoses, indexes.ToArray(), topology, calculateNormals); 
        }

        if (topology == PrimitiveTopology.Point)
        {
            return Create(poses, texPoses, Enumerable.Range(0, poses.Length-1).ToArray(), topology, calculateNormals); 
        }

        throw new ArgumentException($"unknown topology: {topology}");
    }   

    public static Vector3[] GenerateSmoothNormals(Position[] vertices, int[] indexes)
    {
        if (vertices.Length <= 2)
        {
            Logger.Error(typeof(Mesh), "trying to generate smooth normals for < 3 vertices");
            return Enumerable.Repeat(Vector3.UnitX, vertices.Length).ToArray();
        }

        if (indexes.Length % 3 != 0)
        {
            Logger.Error(typeof(Mesh), "trying to generate smooth normals for indexes length % 3 != 0");
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
    {
        if (meshes.Length == 0) 
            return Empty(PrimitiveTopology.Triangle);
        var sameTopology = meshes.All(m => m.Topology == meshes[0].Topology);
        if (!sameTopology)
        {
            Logger.Error(typeof(Mesh), "Can not merge meshes: topologies are different");
            return Empty(meshes[0].Topology);
        }

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
        return new Mesh(vertices, perVertexData, indexes.ToArray(), meshes[0].Topology);
    }
    
    public static bool IsValid(
        PrimitiveTopology topology,
        Position[] vertices,
        PerVertex[] perVertexData,
        int[] indexes,
        out Error? error)
    {
        if (topology.PrimitivesAmountFromIndexes(indexes.Length) <= 0)
        {
            error = "primitives amount is 0";
            return false;
        }

        if (vertices.Length != perVertexData.Length)
        {
            error = $"vertices and {nameof(perVertexData)} sizes aren't equal: {vertices.Length} != {perVertexData.Length}";
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

        for (int i = 0; i < vertices.Length; i++)
        {
            if (indexes.Contains(i)) continue;
            error = $"unused vertex at: {i}";
            return false;
        }

        error = null;
        return true;
    }
}