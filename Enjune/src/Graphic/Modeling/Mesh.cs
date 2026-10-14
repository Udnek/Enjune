using System.Diagnostics.Contracts;
using Enjune.Attribute;
using Enjune.Misc;

namespace Enjune.Graphic.Modeling;

public interface IPositionedVertex
{
    public Position Position { get; set; }
}

public interface ITexturedVertex
{
    public TexturePos Uv { get; set; }
}


/// <summary>
/// Only geometry
/// </summary>
[LogParams(logCallingMethod: true)]
public sealed class Mesh<TPerVert, TPerPrim> where TPerVert : struct where TPerPrim : struct
{
    public readonly PrimitiveTopology Topology;
    public readonly List<TPerVert> Vertices;
    public readonly List<TPerPrim> PerPrimitives;
    public readonly List<int> Indexes;

    public Mesh(int verCap, int perPrimCap, int indCap, PrimitiveTopology topology)
    {
        Topology = topology;
        Vertices = new List<TPerVert>(verCap);
        PerPrimitives = new List<TPerPrim>(perPrimCap);
        Indexes = new List<int>(indCap);
    }

    public void TrimExcess()
    {
        Vertices.TrimExcess();
        PerPrimitives.TrimExcess();
        Indexes.TrimExcess();
    }
    
    public void AddNgon(TPerPrim perPrim, params ReadOnlySpan<TPerVert> newVerts)
    {
        var oldIndexes = Indexes.Count;
        var firstNew = Vertices.Count;
        // adding new vertices
        Vertices.AddRange(newVerts);
        var lastNew = Vertices.Count-1;
        
        if (Topology == PrimitiveTopology.Triangle)
        {
            for (var i = firstNew + 1; i < lastNew; i++)
            {
                // fan-like
                Indexes.Add(firstNew);
                Indexes.Add(i);
                Indexes.Add(i + 1);
            }
        }
        else if (Topology == PrimitiveTopology.Line)
        {
            for (var i = firstNew; i < lastNew; i++)
            {
                Indexes.Add(i);
                Indexes.Add(i+1);
            }
            // loping last -> first
            Indexes.Add(lastNew);
            Indexes.Add(firstNew);
        }
        else if (Topology == PrimitiveTopology.LineStrip)
        {
            for (var i = firstNew; i <= lastNew; i++)
            {
                Indexes.Add(i);
            }
            // loping to firstNew
            Indexes.Add(firstNew);
        }
        else if (Topology == PrimitiveTopology.Point)
        {
            for (int i = firstNew; i <= lastNew; i++) 
                Indexes.Add(i);
        }
        else
        {
            throw new ArgumentException($"unknown topology: {Topology}");
        }

        var newPrimitives = Topology.PrimitivesAmountFromIndexes(Indexes.Count - oldIndexes);
        for (int _ = 0; _ < newPrimitives; _++) 
            PerPrimitives.Add(perPrim);
    }

    [Pure]
    public Mesh<TPerVertNew, TPerPrimNew> Convert<TPerVertNew, TPerPrimNew>(
        Func<TPerVert, TPerVertNew> vertexConvert,
        Func<TPerPrim, TPerPrimNew> primitiveConvert) where TPerVertNew : struct where TPerPrimNew : struct
    {
        var newMesh = new Mesh<TPerVertNew, TPerPrimNew>(
            Vertices.Count, 
            PerPrimitives.Count, 
            Indexes.Count,
            Topology);

        // converting vertices
        for (var i = 0; i < Vertices.Count; i++) 
            newMesh.Vertices[i] = vertexConvert(Vertices[i]);
        
        // converting primitives
        for (var i = 0; i < PerPrimitives.Count; i++) 
            newMesh.PerPrimitives[i] = primitiveConvert(PerPrimitives[i]);
        
        // coping indexes
        newMesh.Indexes.AddRange(Indexes);
        
        return newMesh;
    }
    
    public Error? IsValid()
    {
        var expectedPrimLen = Topology.PrimitivesAmountFromIndexes(Indexes.Count);
        if (expectedPrimLen != PerPrimitives.Count)
        {
            return $"according to {Topology} expected amount of primitives is {expectedPrimLen}, but got {PerPrimitives.Count}";
        }
        
        var min = Indexes.Min();
        if (min != 0)
        {
            return $"index must be > 0, but got: {min}";
        }

        var max = Indexes.Max();
        if (max >= Vertices.Count)
        {
            return  $"index must be < lenght of vertices ({Vertices.Count}), but got: {max}";
        }

        for (int i = 0; i < Vertices.Count; i++)
        {
            if (Indexes.Contains(i)) continue;
            return $"unused vertex at: {i}";
        }
        
        return null;
    }
}

public static class Mesh
{
    public static void AddTriangle<TPerVert, TPerPrim>(
        this Mesh<TPerVert, TPerPrim> mesh,
        TPerVert vertexExample, TPerPrim perPrim,
        Position bl, Position br, Position tr,
        TextureQuad texture) where TPerVert : struct, IPositionedVertex, ITexturedVertex where TPerPrim : struct
    {
        mesh.AddNgon(perPrim, 
            vertexExample with {Position = bl, Uv = texture.BotLeft},
            vertexExample with {Position = br, Uv = texture.BotRight},
            vertexExample with {Position = tr, Uv = texture.TopRight}
        );
    }
    
    public static void AddQuad<TPerVert, TPerPrim>(
        this Mesh<TPerVert, TPerPrim> mesh,
        TPerVert vertexExample, TPerPrim perPrim,
        Position bl, Position br, Position tr, Position tl,
        TextureQuad texture) where TPerVert : struct, IPositionedVertex, ITexturedVertex where TPerPrim : struct
    {
        mesh.AddNgon(perPrim, 
            vertexExample with {Position = bl, Uv = texture.BotLeft},
            vertexExample with {Position = br, Uv = texture.BotRight},
            vertexExample with {Position = tr, Uv = texture.TopRight},
            vertexExample with {Position = tl, Uv = texture.TopLeft}
        );
    }
    
    // all must be counter-clock-wise
    public static void AddHexahedron<TPerVert, TPerPrim>(
        this Mesh<TPerVert, TPerPrim> mesh,
        TPerVert vertexExample, TPerPrim perPrim,
        Position b1, Position b2, Position b3, Position b4,
        Position t1, Position t2, Position t3, Position t4,
        TextureQuad texture) where TPerVert : struct, IPositionedVertex, ITexturedVertex where TPerPrim : struct
    {
        mesh.AddQuad(vertexExample, perPrim, b1, b4, b3, b2, texture); // bottom (−Y)
        mesh.AddQuad(vertexExample, perPrim, t1, t2, t3, t4, texture); // top    (+Y)
        mesh.AddQuad(vertexExample, perPrim, b1, b2, t2, t1, texture); // front  (+Z)
        mesh.AddQuad(vertexExample, perPrim, b2, b3, t3, t2, texture); // right  (+X)
        mesh.AddQuad(vertexExample, perPrim, b3, b4, t4, t3, texture); // back   (−Z)
        mesh.AddQuad(vertexExample, perPrim, b4, b1, t1, t4, texture); // left   (−X)
    }

    public static void AddCuboid<TPerVert, TPerPrim>(
        this Mesh<TPerVert, TPerPrim> mesh,
        TPerVert vertexExample, TPerPrim perPrim,
        Position corner1, Position corner2,
        TextureQuad texture) where TPerVert : struct, IPositionedVertex, ITexturedVertex where TPerPrim : struct
    {
        var min = Vector3.ComponentMin(corner1, corner2);
        var max = Vector3.ComponentMax(corner1, corner2);
        var (x, y, z) = max - min;
        mesh.AddHexahedron(vertexExample, perPrim,
            min, min + (0, 0, z), min + (x, 0, z), min + (x, 0, 0),
            min+(0, y, 0), min + (0, y, z), min + (x, y, z), min + (x, y, 0),
            texture);
    }

    public static void AddCube<TPerVert, TPerPrim>(
        this Mesh<TPerVert, TPerPrim> mesh,
        TPerVert vertexExample, TPerPrim perPrim,
        Position center, float size, 
        TextureQuad texture) where TPerVert : struct, IPositionedVertex, ITexturedVertex where TPerPrim : struct
    {
        var hs = size / 2;
        mesh.AddHexahedron(
            vertexExample, perPrim,
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

            texture);
    }
    
    public static void AddQuadXy<TPerVert, TPerPrim>(
        this Mesh<TPerVert, TPerPrim> mesh,
        TPerVert vertexExample, TPerPrim perPrim,
        Position bl, float width, float height, 
        TextureQuad texture) where TPerVert : struct, IPositionedVertex, ITexturedVertex where TPerPrim : struct
    {
        mesh.AddQuad(
            vertexExample, perPrim,
            bl, bl + (width, 0, 0), bl + (width, height, 0), bl + (0, height, 0), 
            texture);
    }
    
    public static (Vector3 Min, Vector3 Max) Bounds<TPerVert, TPerPrim>(this Mesh<TPerVert, TPerPrim> mesh) where TPerVert : struct, IPositionedVertex where TPerPrim : struct
    {
        var vertices = mesh.Vertices;
        if (vertices.Count == 0)
            return (Vector3.Zero, Vector3.Zero);
        
        var min = vertices[0].Position;
        var max = vertices[0].Position;
        for (var i = 1; i < vertices.Count; i++)
        {
            var vertex = vertices[i];
            max = Vector3.ComponentMax(max, vertex.Position);
            min = Vector3.ComponentMin(min, vertex.Position);
        }
    
        return (min, max);
    }
    
    public static void Move<TPerVert, TPerPrim>(this Mesh<TPerVert, TPerPrim> mesh, Position offset) where TPerVert : struct, IPositionedVertex where TPerPrim : struct
    {
        for (var i = 0; i < mesh.Indexes.Count; i++)
        {
            var vertex = mesh.Vertices[i];
            vertex.Position += offset;
            mesh.Vertices[i] = vertex;
        }
    }
}