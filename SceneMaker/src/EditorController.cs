using System.Numerics;
using System.Runtime.CompilerServices;
using Enjune.Graphic;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.InputHandler;
using Enjune.Misc;
using Enjune.World;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SceneMaker;

public class EditorController
{
    private readonly IGraphicApi _graphicApi;
    private readonly BasicInputHandler _inputHandler;
    private readonly Scene _scene;
    private readonly KeyBinds.Bind _selectBind;
    public SObject? SelectedObject { get; private set; }

    public EditorController(IGraphicApi graphicApi, BasicInputHandler inputHandler, Scene scene)
    {
        _graphicApi = graphicApi;
        _inputHandler = inputHandler;
        _scene = scene;
        _selectBind = new KeyBinds.Bind("select", UniKey.Of(MouseButton.Left));
        _inputHandler._binds.AddBind(_selectBind);
    }

    public void Update(Matrix4 viewMat, Matrix4 projMat)
    {
        if (!_inputHandler.IsPressed(_selectBind)) return;
        SelectedObject = Trace(_graphicApi.GetWindowSize(), viewMat, projMat);
    }

    private SObject? Trace(Vector2i screenSize, Matrix4 viewMat, Matrix4 projMat)
    {
        var cursorPosition = _inputHandler.CursorPosition;
        if (_graphicApi.GetCursorMode() == IGraphicApi.CursorMode.Centered)
        {
            cursorPosition = screenSize / 2;
        }
        return Trace(screenSize, cursorPosition, viewMat, projMat);
    }

    private SObject? Trace(Vector2i screenSize, Vector2i rawCursorPos, Matrix4 viewMat, Matrix4 projMat)
    {
        Vector2 cursorPos = (rawCursorPos.X, screenSize.Y - rawCursorPos.Y);
        var ndc = new Vector2(
            (cursorPos.X / screenSize.X) * 2f - 1f,
            (cursorPos.Y / screenSize.Y) * 2f - 1f
        );
        
        var nearClip = new Vector4(ndc.X, ndc.Y, -1f, 1f);
        var farClip  = new Vector4(ndc.X, ndc.Y, 1f, 1f);
        
        var unProject = (projMat.Transposed() * viewMat.Transposed()).Inverted(); // we must transpose this shit
        var nearWorld = unProject * nearClip;
        var farWorld = unProject * farClip;
        
        nearWorld /= nearWorld.W;
        farWorld /= farWorld.W;
        
        var direction = (farWorld - nearWorld).Xyz.Normalized();

        var cameraPos = viewMat.Inverted().ExtractTranslation();

        SObject? closest =  null;
        var closestDistance = float.MaxValue;
        foreach (var obj in _scene.Objects)
        {
            if (!Trace(cameraPos, direction, obj, out var distance)) continue;
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closest = obj;
        }
        return closest;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Trace(Vector3 cameraPos, Vector3 direction, SObject obj, out float distance)
    {
        var modelMatInv = obj.ModelMatrix.Inverted();
        
        var localDirection = modelMatInv.TransformDirection(direction).Normalized();
        var localCameraPos = modelMatInv.TransformPosition(cameraPos);
        
        foreach (var (mesh, _) in obj.Model.Meshes)
        {
            for (var indexIndex = 0; indexIndex < mesh.Indexes.Length; indexIndex+=3)
            {
                var verIndex0 = mesh.Indexes[indexIndex];
                var verIndex1 = mesh.Indexes[indexIndex+1];
                var verIndex2 = mesh.Indexes[indexIndex+2];
                if (RayIntersectionDistance(localCameraPos, localDirection,
                        mesh.Vertices[verIndex0], mesh.Vertices[verIndex1], mesh.Vertices[verIndex2], out distance))
                {
                    return true;
                }
            }
        }

        distance = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool RayIntersectionDistance(Position origin, Vector3 dir, Position p0, Position p1, Position p2, out float distance)
    {
        distance = 0;
        
        var e1 = p1 - p0;
        var e2 = p2 - p0;

        var pvec = Vector3.Cross(dir, e2);
        var det = Vector3.Dot(e1, pvec);

        // parallel
        if (det < 1e-8 && det > -1e-8) 
            return false;

        var invDet = 1 / det;
        var tvec = origin - p0;
        var u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0 || 1 < u) 
            return false;

        var qvec = Vector3.Cross(tvec, e1);
        float v = Vector3.Dot(dir, qvec) * invDet;
        if (v < 0 || 1 < u + v)
            return false;
        
        distance = Vector3.Dot(e2, qvec) * invDet;
        return true;
    }
}