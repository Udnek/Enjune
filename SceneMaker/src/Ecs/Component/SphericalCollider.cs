using System;
using System.Collections.Generic;
using Enjune.Ecs.Component;
using System.Text;

namespace SceneMaker.Ecs.Component;

public record struct SphericalCollider(Double Radius, Double Stiffness) : IComponent;
