using System.Runtime.CompilerServices;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;
using Enjune.Registering;
using Enjune.World;
using OpenTK.Mathematics;
namespace SceneMaker;

public class EditorController
{
    private readonly IGraphicApi _graphicApi;
    private readonly BasicInputHandler _inputHandler;
    private readonly Scene _scene;
    private readonly KeyBinds.Bind _selectBind;
    public SObject? SelectedObject { get; private set; }
    private readonly Dictionary<Mesh, Ax> _meshToAx = new(3);
    private Ax? _selectedAx;

    public readonly SObject AxisObject;

    public EditorController(IGraphicApi graphicApi, BasicInputHandler inputHandler, Scene scene)
    {
        _graphicApi = graphicApi;
        _inputHandler = inputHandler;
        _scene = scene;
        _selectBind = new KeyBinds.Bind("select", KeyCode.LeftMouseButton);

        var x = new Mesh([Vector3.Zero, Vector3.UnitX], [default, default], [0, 1]);
        var y = new Mesh([Vector3.Zero, Vector3.UnitY], [default, default], [0, 1]);
        var z = new Mesh([Vector3.Zero, Vector3.UnitZ], [default, default], [0, 1]);
        var model = RegistrableModel.CreateAndRegister(
            new Identifier(this.GetAssembly(), "axis"),
            new Model.Builder()
                .Add(x, new Model.PerMesh(new Color(1f, 0f, 0f, 1f)))
                .Add(y, new Model.PerMesh(new Color(0f, 1f, 0f, 1f)))
                .Add(z, new Model.PerMesh(new Color(0f, 0f, 1f, 1f)))
                .Build(false)
            );
        AxisObject = new SObject
        {
            IsRealistic = false,
            Model = model,
            RenderableModel = _graphicApi.CreateStaticRenderable(model.Model, IGraphicApi.Primitive.Line),
            Hidden = true,
            Scale = new Vector3(2.5f),
            ToBeSerialized = false,
        };
        _meshToAx[x] = Ax.X;
        _meshToAx[y] = Ax.Y;
        _meshToAx[z] = Ax.Z;
    }

    public void Update(Matrix4 viewMat, Matrix4 projMat)
    {
        if (SelectedObject != null) 
            UpdateSelectedObject(viewMat, projMat);
        
        if (_selectedAx != null) return; // don't need to trace anything
        if (!_inputHandler.IsPressed(_selectBind)) return;
        if (SelectedObject != null)
        {
            // trying to trace axis first
            var mesh = TraceLineObject(AxisObject, viewMat, projMat, 5);
            if (mesh != null)
            {
                _selectedAx = _meshToAx[mesh];
                // don't need to trace anything else
                return;
            }
        }
        SelectedObject = TraceObjects(viewMat, projMat);
        AxisObject.Hidden = SelectedObject == null;
    }
    
    private enum Ax
    {
        X, Y, Z
    }

    private Vector3 AxToVec(Ax ax)
    {
        return ax switch
        {
            Ax.X => AxisObject.Rotation * Vector3.UnitX,
            Ax.Y => AxisObject.Rotation * Vector3.UnitY,
            Ax.Z => AxisObject.Rotation * Vector3.UnitZ,
            _ => throw new ArgumentOutOfRangeException(nameof(ax), ax, null)
        };
    }

    private void UpdateSelectedObject(Matrix4 viewMat, Matrix4 projMat)
    {
        if (SelectedObject == null) return;
        if (_inputHandler.IsJustReleased(_selectBind))
            _selectedAx = null;
        else if (_selectedAx != null) 
            DragObject(viewMat, projMat);
        
        AxisObject.Position = SelectedObject.Position;
        AxisObject.Rotation = SelectedObject.Rotation;
    }

    private void DragObject(Matrix4 viewMat, Matrix4 projMat)
    {
        if (SelectedObject == null || _selectedAx == null) return;
        if (_inputHandler.DeltaCursorPosition == (0, 0)) return;
        GetCursorVectors(viewMat, projMat, out var direction, out var camPos);
        
        var axToVec = AxToVec((Ax)_selectedAx);

        if (!Do(axToVec))
            Do(-1 * axToVec);
        return;    
        
        bool Do(Vector3 axDir)
        {
            var projectedDir = MathUtils.ProjectVectorOnPlane(direction, camPos, AxisObject.Position, AxisObject.Position + axDir);
            if (!MathUtils.VectorsIntersect(camPos, projectedDir,
                    AxisObject.Position, axDir, out var intersection)) return false;
            SelectedObject!.Position = intersection;
            return true;
        }
    }

    private Vector2 GetNdcCursorPosition()
    {
        var screenSize = _graphicApi.GetWindowSize();
        Vector2 cursorPos;
        if (_graphicApi.GetCursorMode() == IGraphicApi.CursorMode.Centered) 
            cursorPos = screenSize / 2;
        else
            cursorPos = _inputHandler.CursorPosition;

        var ndc = cursorPos / screenSize * 2f - (1, 1);
        return ndc;
    }

    private void GetCursorVectors(Matrix4 viewMat, Matrix4 projMat, out Vector3 direction, out Vector3 cameraPos)
    {
        var ndc = GetNdcCursorPosition();
        var nearClip = new Vector4(ndc.X, ndc.Y, -1f, 1f);
        var farClip  = new Vector4(ndc.X, ndc.Y, 1f, 1f);
        var unProject = (projMat.Transposed() * viewMat.Transposed()).Inverted(); // we must transpose this shit
        var nearWorld = unProject * nearClip;
        var farWorld = unProject * farClip;
        
        nearWorld /= nearWorld.W;
        farWorld /= farWorld.W;
        
        direction = (farWorld - nearWorld).Xyz.Normalized();
        cameraPos = viewMat.Inverted().ExtractTranslation();
    }
    
    private Mesh? TraceLineObject(SObject obj, Matrix4 viewMat, Matrix4 projMat, float minimumAngleDegrees)
    {
        if (obj.Model is null) return null;
        GetCursorVectors(viewMat, projMat, out var direction, out var cameraPos);
        
        var modelMatInv = obj.ModelTransform.Inverted();
        
        direction = modelMatInv.TransformDirection(direction).Normalized();
        cameraPos = modelMatInv.TransformPosition(cameraPos);

        Mesh? nearestMesh = null;
        var minDist = float.MaxValue;
        foreach (var pair in obj.Model.Model.Meshes)
        {
            var mesh = pair.Mesh;
            for (var indexIndex = 0; indexIndex < mesh.Indexes.Length; indexIndex+=2)
            {
                var verIndex0 = mesh.Indexes[indexIndex];
                var verIndex1 = mesh.Indexes[indexIndex+1];
                if (!MathUtils.RayIntersectsLine(cameraPos, direction, mesh.Vertices[verIndex0], mesh.Vertices[verIndex1], out var cosAngle))
                    continue;
                
                var degAngle = MathHelper.RadiansToDegrees(MathF.Acos(cosAngle));
                if (degAngle < minimumAngleDegrees && degAngle < minDist)
                {
                    nearestMesh = mesh;
                    minDist = degAngle;
                }
            }
        }
        return nearestMesh;
    }

    private SObject? TraceObjects(Matrix4 viewMat, Matrix4 projMat)
    {
        GetCursorVectors(viewMat, projMat, out var direction, out var cameraPos);

        SObject? closest =  null;
        var closestDistance = float.MaxValue;
        foreach (var obj in _scene.Objects)
        {
            if (!TraceObject(cameraPos, direction, obj, out var distance)) continue;
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closest = obj;
        }
        return closest;
    }
    
    private static bool TraceObject(Vector3 cameraPos, Vector3 direction, SObject obj, out float distance)
    {
        if (obj.Model is null || obj.RenderableModel?.CurrentPrimitive != IGraphicApi.Primitive.Triangle)
        {
            distance = 0;
            return false;
        }
        var modelMatInv = obj.ModelTransform.Inverted();
        
        var localDirection = modelMatInv.TransformDirection(direction).Normalized();
        var localCameraPos = modelMatInv.TransformPosition(cameraPos);
        
        foreach (var (mesh, _) in obj.Model.Model.Meshes)
        {
            for (var indexIndex = 0; indexIndex < mesh.Indexes.Length; indexIndex+=3)
            {
                var verIndex0 = mesh.Indexes[indexIndex];
                var verIndex1 = mesh.Indexes[indexIndex+1];
                var verIndex2 = mesh.Indexes[indexIndex+2];
                if (MathUtils.RayIntersectsTriangle(localCameraPos, localDirection,
                        mesh.Vertices[verIndex0], mesh.Vertices[verIndex1], mesh.Vertices[verIndex2], out distance))
                {
                    return true;
                }
            }
        }

        distance = 0;
        return false;
    }
}