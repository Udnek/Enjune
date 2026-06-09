using System.Numerics;
using System.Runtime.CompilerServices;
using Enjune.Graphic;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.Input;
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
    public readonly KeyBinds.Bind SelectBind;

    public EditorController(IGraphicApi graphicApi, BasicInputHandler inputHandler, Scene scene)
    {
        _graphicApi = graphicApi;
        _inputHandler = inputHandler;
        _scene = scene;
        SelectBind = new KeyBinds.Bind("select", UniKey.Of(MouseButton.Left));
        _inputHandler._binds.AddBind(SelectBind);
    }
    
    public SObject? Trace(Vector2i screenSize, Matrix4 viewMat, Matrix4 projMat)
    {
        var scaledPosition = ((screenSize / 2).ToVector2() / screenSize - (0.5f, 0.5f)) * 2f;
        
        Logger.Log(this, $"scaledpos: {scaledPosition}");
        
        var nearClip = new Vector4(scaledPosition.X, scaledPosition.Y, -1.0f, 1.0f);
        var farClip  = new Vector4(scaledPosition.X, scaledPosition.Y, 1.0f, 1.0f);

        Logger.Log(this, $"view: {viewMat}");
        Logger.Log(this, $"proj: {projMat}");
        
        var unProject = (viewMat * projMat).Inverted();
        nearClip = unProject * nearClip;
        farClip = unProject * farClip;
        
        nearClip /= nearClip.W;
        farClip /= farClip.W;

        var direction = (farClip - nearClip).Xyz.Normalized();

        // direction = unProject.TransformDirection(direction).Normalized();;

        var cameraPos = viewMat.ExtractTranslation();
        
        Logger.Log(this, $"dir: {direction}");
        foreach (var obj in _scene.Objects)
        {
            if (Trace(direction, obj))
            {
                return obj;
            }
        }

        return null;
    }

    private bool Trace(Vector3 dir, SObject obj)
    {
        foreach (var (mesh, _) in obj.Model.Meshes)
        {
            for (var indexIndex = 0; indexIndex < mesh.Indexes.Length; indexIndex+=3)
            {
                var verIndex0 = mesh.Indexes[indexIndex];
                var verIndex1 = mesh.Indexes[indexIndex+1];
                var verIndex2 = mesh.Indexes[indexIndex+2];
                if (RayIntersectionDistance(obj.Position, dir,
                        mesh.Vertices[verIndex0], mesh.Vertices[verIndex1], mesh.Vertices[verIndex2], out var distance))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool RayIntersectionDistance(Position origin, Vector3 dir, Position p0, Position p1, Position p2, out float distance)
    {
        distance = 0;
        
        var e1 = p1 - p0;
        var e2 = p2 - p0;

        var P = Vector3.Cross(dir, e1);
        var det = Vector3.Dot(P, e1);

        // parallel
        if (det < 1e-8 && det > -1e-8) 
            return false;

        var invDet = 1 / det;
        var T = origin - p0;
        var u = Vector3.Dot(T, P) * invDet;
        if (u < 0 || 1 < u) 
            return false;

        var Q = Vector3.Cross(T, e1);
        float v = Vector3.Dot(dir, Q) * invDet;
        if (v < 0 || 1 < u + v)
            return false;
        
        distance = Vector3.Dot(e2, Q) * invDet;
        return true;
    }
}