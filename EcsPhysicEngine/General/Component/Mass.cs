using System;
using System.Collections.Generic;
using System.Text;

namespace PhysicsLibrary.General.Component;
/// <summary>
/// Stores mass as an inverse value (1/mass) due to a significant calculation advantage
/// </summary>
public record struct Mass
{
    private float _inverseMass;
    public float Inverse {
        get { return _inverseMass; }
        set { _inverseMass = value; }
    }
    public float Value
    {
        get { return 1/_inverseMass; }
        set { _inverseMass = 1 / value; }
    }
}
