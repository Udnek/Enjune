using Enjune.Misc;

namespace Enjune.Graphic;

public abstract class AbstractMesh<TPerVertex>
{
    public readonly Position[] Vertices;
    public readonly int[] Indexes;
    public readonly TPerVertex[] PerVertexData;

    protected AbstractMesh(Position[] vertices, TPerVertex[] perVertexData, int[] indexes)
    {
        if (!AbstractMesh.IsValid(vertices, perVertexData, indexes, out var error)) 
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

    public void Multiply(Vector3 vector)
    {
        for (int i = 0; i < Vertices.Length; i++) 
            Vertices[i] *= vector;
    }
}


public static class AbstractMesh
{
    public static bool IsValid<TPerVertex>(Position[] vertices, TPerVertex[] perVertexData, int[] indexes,
        out Error? error)
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