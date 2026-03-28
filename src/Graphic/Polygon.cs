using Enjune.Misc;

namespace Enjune.Graphic;

public struct Polygon(Position v0, Position v1, Position v2)
{
    public Position V0 = v0;
    public Position V1 = v1;
    public Position V2 = v2;
    
    public static void Quad(Position p1, Position p2, Position p3, Position p4, Consumer<Polygon> consumer)
    {
        consumer(new Polygon(p1, p2, p3)); //123
        consumer(new Polygon(p3, p4, p1)); //341
    }
    
    public static void Cuboid(
        Position b1, Position b2, Position b3, Position b4, 
        Position t1, Position t2, Position t3, Position t4, Consumer<Polygon> consumer)
    {
        Quad(b1, b2, b3, b4, consumer); //bottom
        Quad(t1, t2, t3, t4, consumer); //top

        Quad(b1, b2, t2, t1, consumer); //12-21
        Quad(b2, b3, t3, t2, consumer); //23-32
        Quad(b3, b4, t4, t3, consumer); //34-43
        Quad(b4, b1, t1, t4, consumer); //41-14
    }
    
    public static void Cube(Position center, float size, Consumer<Polygon> consumer)
    {
        Cuboid(
            // bottom
            center + (-size / 2, -size / 2, -size / 2), //-x -z
            center + (size / 2, -size / 2, -size / 2), //+x -z
            center + (size / 2, -size / 2, size / 2), //+x +z
            center + (-size / 2, -size / 2, size / 2), //-x +z
            // top
            center + (-size / 2, size / 2, -size / 2), //-x -z
            center + (size / 2, size / 2, -size / 2), //+x -z
            center + (size / 2, size / 2, size / 2), //+x +z
            center + (-size / 2, size / 2, size / 2), //-x +z

            consumer
        );
    }
}