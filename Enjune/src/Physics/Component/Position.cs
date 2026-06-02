namespace Enjune.Physics.Component;

public struct Position : IComponent
{
    public double X;
    public double Y;
    public double Z;

    public Position(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public override string ToString()
    {
        return "(" + X + ", " + Y + ", " + Z + ")";
    }
}