using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace Standoff2.Ecs.Component;

public record struct Position(double X, double Y, double Z) : IComponent
{
    public override string ToString() => "(" + X + ", " + Y + ", " + Z + ")";
}
