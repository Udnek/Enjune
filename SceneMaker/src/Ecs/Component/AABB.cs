using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace SceneMaker.src.Ecs.Component;

/* 
 * Bounding box size is determined by half-sizes,
 * which are deltas from center coordinates.
 * Setting dx, dy and dz to 1 will result in a
 * bounding box that is a 2x2x2 cube.
 */

public record struct AABB(
    double DeltaX,
    double DeltaY, 
    double DeltaZ
    ) : IComponent;
