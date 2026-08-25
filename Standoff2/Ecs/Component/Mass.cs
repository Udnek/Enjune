using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace Standoff2.Ecs.Component;

public record struct Mass(double Value) : IComponent;
