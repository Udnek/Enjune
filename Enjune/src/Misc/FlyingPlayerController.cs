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

    public Position _position = new(0f, 0f, 5f);
    private float _pitch = 0f;
    public float _yaw = 0f;
    
    public FlyingPlayerController(IGraphicApi graphicApi, BasicInputHandler inputHandler, Wasd wasd, float sensitivity)
    {
        _graphicApi = graphicApi;
        _inputHandler = inputHandler;
        _wasd = wasd;
        _sensitivity = sensitivity;
    }

    public Matrix4 View => Matrix4.LookAt(_position, _position + Direction, Vector3.UnitY);

    public Vector3 Direction
    {
        get
        {
            var radYaw = MathHelper.DegreesToRadians(_yaw);
            var radPitch = MathHelper.DegreesToRadians(_pitch);
            var cosPitch = MathF.Cos(radPitch);
            return new Vector3(
                MathF.Sin(radYaw) * cosPitch,
                MathF.Sin(radPitch), 
                -MathF.Cos(radYaw) * cosPitch
                ).Normalized();
        }
    }

    public void Update(float deltaTime)
    {
        if (_graphicApi.GetCursorMode() == IGraphicApi.CursorMode.Centered)
        {
            _yaw += _sensitivity * _inputHandler.DeltaMousePosition.X;
            _pitch -= _sensitivity * _inputHandler.DeltaMousePosition.Y;
            _yaw %= 360;
            _pitch = Math.Clamp(_pitch, -89f, 89f);
        }
        
        // todo rewrite using sins and cosines instead of matrix4 
        var cos = MathF.Cos(MathHelper.DegreesToRadians(_yaw));
        var sin = MathF.Sin(MathHelper.DegreesToRadians(_yaw));
        var forward = Direction;
        forward.Y = 0;
        forward.Normalize();
        var right = Vector3.Cross(forward, Vector3.UnitY); 

        // movement input
        var move = new Vector3(0f, 0f, 0f);
        if (_inputHandler.IsPressed(_wasd.Forward))
            move += forward;
        else if (_inputHandler.IsPressed(_wasd.Backward))
            move += forward * -1;

        if (_inputHandler.IsPressed(_wasd.Rightward))
            move += right;
        else if (_inputHandler.IsPressed(_wasd.Leftward))
            move += right * -1;

        if (_inputHandler.IsPressed(_wasd.Upward))
            move += new Vector3(0f, 1f, 0f);
        else if (_inputHandler.IsPressed(_wasd.Downward))
            move += new Vector3(0f, -1f, 0f);
        
        _position += move * 8f * deltaTime;
    }
}