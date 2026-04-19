namespace Enjune.Graphic.Input.UI;

public class UI
{
    
}

public class UiElement(Vector2 hitbox)
{
    public Vector2 Hitbox = hitbox;
    public Position Position = (0, 0, 0);
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4 ModelMatrix =>
        Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateScale(Scale);
}

public class Button
{
    
}