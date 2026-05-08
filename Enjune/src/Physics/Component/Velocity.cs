namespace Enjune.Physics.Component;

public struct Velocity : IComponent
{
    public double X;
    public double Y;
    public double Z;

    public Velocity(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}