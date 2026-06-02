namespace Enjune.Physics.Component;

public struct Acceleration : IComponent
{
    public double X;
    public double Y;
    public double Z;
    
    public Acceleration(double x, double y, double z)
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