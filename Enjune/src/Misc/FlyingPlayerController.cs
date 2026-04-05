using Enjune.Graphic;
using Enjune.Graphic.InputHandler;
using OpenTK.Mathematics;

namespace Enjune.Misc;

public class FlyingPlayerController
{
    private readonly BasicInputHandler _inputHandler;

    private Position _position = new(0f, 0f, 0f);
    private float _pitch = 0f;
    private float _yaw = 0f;
    
    public FlyingPlayerController(BasicInputHandler inputHandler)
    {
        _inputHandler = inputHandler;
    }

    // todo figure out even why this shit works
    public Matrix4 View => Matrix4.CreateTranslation(-_position) 
                           * Matrix4.CreateRotationY(-MathHelper.DegreesToRadians(_yaw))
                           * Matrix4.CreateRotationX(-MathHelper.DegreesToRadians(_pitch)
    );
    
    
    public void Update(float deltaTime)
    {
        var rotSpeed = 120 * deltaTime;
        if (_inputHandler.IsPressed(KeyBinds.LookLeft)) _yaw += rotSpeed;
        if (_inputHandler.IsPressed(KeyBinds.LookRight)) _yaw -= rotSpeed;
        if (_inputHandler.IsPressed(KeyBinds.LookUp)) _pitch += rotSpeed;
        if (_inputHandler.IsPressed(KeyBinds.LookDown)) _pitch -= rotSpeed;
        _yaw %= 360;
        _pitch = Math.Clamp(_pitch, -90f, 90f);

        var radYaw = MathHelper.DegreesToRadians(_yaw);
        var radPitch = MathHelper.DegreesToRadians(_pitch);
                
        var yawRotation = Matrix4.CreateRotationY(radYaw);
        //var pitchRotation = Matrix4.CreateRotationX(radPitch);
        //var cameraRotation = yawRotation * pitchRotation;

        // movement input
        var move = new Vector3(0f, 0f, 0f);
        if (_inputHandler.IsPressed(KeyBinds.Forward))
            move += yawRotation.TransformDirection(new Vector3(0f, 0f, -1f));
        else if (_inputHandler.IsPressed(KeyBinds.Backward)) 
            move += yawRotation.TransformDirection(new Vector3(0f, 0f, 1f));

        if (_inputHandler.IsPressed(KeyBinds.Rightward))
            move += yawRotation.TransformDirection(new Vector3(1f, 0f, 0f));
        else if (_inputHandler.IsPressed(KeyBinds.Leftward)) 
            move += yawRotation.TransformDirection(new Vector3(-1f, 0f, 0f));

        if (_inputHandler.IsPressed(KeyBinds.Upward))
            move += new Vector3(0f, 1f, 0f);
        else if (_inputHandler.IsPressed(KeyBinds.Downward)) 
            move += new Vector3(0f, -1f, 0f);
                
        _position += move * 8f * deltaTime;
    }
}