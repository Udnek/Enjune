using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;
using Enjune.Registering;
using Enjune.World;
using OpenTK.Mathematics;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;
using SceneMaker.Misc;

namespace SceneMaker.Ecs.System;

public class EditorSystem : SingleQuerySystem
{
    private readonly IGraphicApi _graphicApi;
    private readonly BasicInputHandler _inputHandler;
    private readonly GraphicBridge _bridge;
    private readonly KeyBinds.Bind _selectBind;
    private readonly Dictionary<Mesh, Ax> _meshToAx = new(3);
    private World _world = null!;
    
    public Entity? SelectedEntity { get; private set; }
    private Ax? _selectedAx;
    private GraphicObject _axisObject;

    public EditorSystem(IGraphicApi graphicApi, BasicInputHandler inputHandler, GraphicBridge bridge)
    {
        _graphicApi = graphicApi;
        _inputHandler = inputHandler;
        _bridge = bridge;
        _selectBind = new KeyBinds.Bind("select", KeyCode.LeftMouseButton);

        #region Constructing Axis Obj
        {
            var x = new Mesh([Vector3.Zero, Vector3.UnitX], [default, default], [0, 1]);
            var y = new Mesh([Vector3.Zero, Vector3.UnitY], [default, default], [0, 1]);
            var z = new Mesh([Vector3.Zero, Vector3.UnitZ], [default, default], [0, 1]);
            _axisObject = new GraphicObject()
            {
                DropsShadow = false,
                Model = _graphicApi.CreateStaticRenderable(
                    new Model.Builder()
                        .Add(x, new Model.PerMesh(new Color(1f, 0f, 0f, 1f)))
                        .Add(y, new Model.PerMesh(new Color(0f, 1f, 0f, 1f)))
                        .Add(z, new Model.PerMesh(new Color(0f, 0f, 1f, 1f)))
                        .Build(false), IGraphicApi.Primitive.Line),
                IsHidden = true,
            };
            _meshToAx[x] = Ax.X;
            _meshToAx[y] = Ax.Y;
            _meshToAx[z] = Ax.Z;   
        }
        #endregion
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        _world = world;
    }

    protected override Query BuildQuery(Query.Builder builder)
    {
        return builder
            .With<ModelComponent>()
            .With<Transform>().Build();
    }

    public override void Update()
    {
        
    }

    public void Update(Vector3 camPos, Vector3 camDir)
    {
        if (SelectedEntity != null) 
            UpdateSelectedEntity(camPos, camDir);
        
        if (_selectedAx != null) return; // don't need to trace anything
        if (!_inputHandler.IsPressed(_selectBind)) return;
        if (SelectedEntity != null)
        {
            // trying to trace axis first
            var mesh = EditorMisc.TraceLineObject(_axisObject, viewMat, projMat, 5);
            if (mesh != null)
            {
                _selectedAx = _meshToAx[mesh];
                // don't need to trace anything else
                return;
            }
        }
        SelectedEntity = TraceObjects(viewMat, projMat);
        _axisObject.Hidden = SelectedEntity == null;
    }
    
    private void UpdateSelectedEntity(Vector3 camPos, Vector3 camDir)
    {
        if (SelectedEntity == null) return;
        if (_inputHandler.IsJustReleased(_selectBind))
            _selectedAx = null;
        else if (_selectedAx != null) 
            DragSelectedEntity(camPos, camDir);
        
        _axisObject.Position = SelectedObject.Position;
        _axisObject.Rotation = SelectedObject.Rotation;
    }

    private void DragSelectedEntity(Vector3 camPos, Vector3 camDir)
    {
        if (SelectedEntity == null || _selectedAx == null) return;
        if (_inputHandler.DeltaCursorPosition == (0, 0)) return;
        
        var axToVec = AxToVec(_selectedAx.Value);

        if (!Do(axToVec))
            Do(-1 * axToVec);
        return;    
        
        bool Do(Vector3 axDir)
        {
            var axPos = _axisObject.TransformMatrix.ExtractTranslation();
            var projectedDir = MathUtils.ProjectVectorOnPlane(camDir, camPos, axPos, axPos + axDir);
            
            if (!MathUtils.VectorsIntersect(camPos, projectedDir, axPos, axDir, out var intersection))
                return false;

            
            SelectedObject!.Position = intersection;
            return true;
        }
    }
    
    private Entity? TraceObjects(Vector3 camPos, Vector3 camDir)
    {
        Entity? closest = null;
        var closestDistance = float.MaxValue;
        Query.ForEachArchetype(archetype =>
        {
            var transforms = archetype.GetComponents<Transform>();
            var models = archetype.GetComponents<ModelComponent>();
            for (int row = 0; row < archetype.Rows; row++)
            {
                if (!EditorMisc.TraceObject(camPos, camDir, models[row], transforms[row], out var distance)) continue;
                if (distance >= closestDistance) continue;
                closestDistance = distance;
                closest = archetype.GetEntityByRow(row);
            }
        });
        return closest;
    }

    #region Getting Vectors
    
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

    private void GetCursorVectors(Matrix4 viewMat, Matrix4 projMat, out Vector3 camPos, out Vector3 camDir)
    {
        var ndc = GetNdcCursorPosition();
        var nearClip = new Vector4(ndc.X, ndc.Y, -1f, 1f);
        var farClip  = new Vector4(ndc.X, ndc.Y, 1f, 1f);
        var unProject = (projMat.Transposed() * viewMat.Transposed()).Inverted(); // we must transpose this shit
        var nearWorld = unProject * nearClip;
        var farWorld = unProject * farClip;
        
        nearWorld /= nearWorld.W;
        farWorld /= farWorld.W;
        
        camPos = viewMat.Inverted().ExtractTranslation();
        camDir = (farWorld - nearWorld).Xyz.Normalized();
    }
    
    #endregion
    
    #region Ax

    private enum Ax
    {
        X, Y, Z
    }

    private Vector3 AxToVec(Ax ax)
    {
        return ax switch
        {
            Ax.X => _axisObject.TransformMatrix.ExtractRotation() * Vector3.UnitX,
            Ax.Y => _axisObject.TransformMatrix.ExtractRotation() * Vector3.UnitY,
            Ax.Z => _axisObject.TransformMatrix.ExtractRotation() * Vector3.UnitZ,
            _ => throw new ArgumentOutOfRangeException(nameof(ax), ax, null)
        };
    }

    #endregion
}