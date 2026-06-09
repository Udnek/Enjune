using Enjune.Graphic;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.Input;
using OpenTK.Mathematics;

namespace Enjune.Misc;

public class FlyingPlayerController
{
    private readonly IGraphicApi _graphicApi;
    private readonly BasicInputHandler _inputHandler;
    private readonly Wasd _wasd;
    private readonly float _sensitivity;

    private Position _position = new(0f, 0f, 0f);
    private float _pitch = 0f;
    private float _yaw = 0f;
    
    public FlyingPlayerController(IGraphicApi graphicApi, BasicInputHandler inputHandler, Wasd wasd, float sensitivity)
    {
        _graphicApi = graphicApi;
        _inputHandler = inputHandler;
        _wasd = wasd;
        _sensitivity = sensitivity;
    }

    // todo figure out even why this shit works
    public Matrix4 View => Matrix4.CreateTranslation(-_position) 
                           * Matrix4.CreateRotationY(-MathHelper.DegreesToRadians(_yaw))
                           * Matrix4.CreateRotationX(-MathHelper.DegreesToRadians(_pitch)
    );
    
    
    public void Update(float deltaTime)
    {
        if (_graphicApi.GetCursorMode() == IGraphicApi.CursorMode.Centered)
        {
            _yaw -= _sensitivity * (float)_inputHandler.DeltaMousePosition.X;
            _pitch -= _sensitivity * (float) _inputHandler.DeltaMousePosition.Y;
            _yaw %= 360;
            _pitch = Math.Clamp(_pitch, -90f, 90f);
        }
        
        // todo rewrite using sins and cosines instead of matrix4 
        var yawRotation = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_yaw));

        // movement input
        var move = new Vector3(0f, 0f, 0f);
        if (_inputHandler.IsPressed(_wasd.Forward))
            move += yawRotation.TransformDirection(new Vector3(0f, 0f, -1f));
        else if (_inputHandler.IsPressed(_wasd.Backward)) 
            move += yawRotation.TransformDirection(new Vector3(0f, 0f, 1f));

        if (_inputHandler.IsPressed(_wasd.Rightward))
            move += yawRotation.TransformDirection(new Vector3(1f, 0f, 0f));
        else if (_inputHandler.IsPressed(_wasd.Leftward)) 
            move += yawRotation.TransformDirection(new Vector3(-1f, 0f, 0f));

        if (_inputHandler.IsPressed(_wasd.Upward))
            move += new Vector3(0f, 1f, 0f);
        else if (_inputHandler.IsPressed(_wasd.Downward)) 
            move += new Vector3(0f, -1f, 0f);
                
        _position += move * 8f * deltaTime;
    }
}