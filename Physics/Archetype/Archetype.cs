using System.Globalization;
using Enjune.Physics.Type;

namespace Enjune.Physics.Archetype;

public class Archetype
{
    public Signature Signature;
    public Array[] ComponentArrays;
    public int[] Entities;
    public int Count;

    public Archetype(Signature signature)
    {
        Signature = signature;
        ComponentArrays = new Array[21];
        Entities = new int[10];
    }
}