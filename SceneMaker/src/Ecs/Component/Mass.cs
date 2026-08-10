using Enjune.Ecs.Component;
using System.Transactions;

namespace SceneMaker.Ecs.Component;

public record struct Mass(double Value) : IComponent;