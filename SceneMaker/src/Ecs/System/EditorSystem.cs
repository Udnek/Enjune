using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;
using SceneMaker.Misc;

namespace SceneMaker.Ecs.System;

public class EditorSystem : ISystem
{
    private readonly App _app;
    private readonly KeyBinds.Bind _selectBind;
    private readonly KeyBinds.Bind _selectSeveralBind;
    
    private readonly Dictionary<Mesh, Ax> _meshToAx = new(3);
    private World _world = null!;
    private Query _allQuery = null!;
    private Query _selectedQuery = null!;
    
    //public Entity? SelectedEntity { get; private set; }
    private Ax? _selectedAx;
    private GraphicObject _axisObject;
    private readonly Model _axisModel;
    private const float AxisSize = 2;

    public EditorSystem(App app)
    {
        _app = app;
        _selectBind = new KeyBinds.Bind("select", KeyCode.LeftMouseButton);
        _selectSeveralBind = new KeyBinds.Bind("select_several", KeyCode.LeftShift);

        #region Constructing Axis Obj
        {
            var x = new Mesh([Vector3.Zero, Vector3.UnitX], [default, default], [0, 1]);
            var y = new Mesh([Vector3.Zero, Vector3.UnitY], [default, default], [0, 1]);
            var z = new Mesh([Vector3.Zero, Vector3.UnitZ], [default, default], [0, 1]);
            _axisModel = new Model.Builder()
                .Add(x, new Model.PerMesh(new Color(1f, 0f, 0f, 1f)))
                .Add(y, new Model.PerMesh(new Color(0f, 1f, 0f, 1f)))
                .Add(z, new Model.PerMesh(new Color(0f, 0f, 1f, 1f)))
                .Build(false);
            _axisObject = new GraphicObject(app.GraphicApi.CreateStaticRenderable(_axisModel, IGraphicApi.Primitive.Line))
            {
                DropsShadow = false,
                IsHidden = true
            };
            _meshToAx[x] = Ax.X;
            _meshToAx[y] = Ax.Y;
            _meshToAx[z] = Ax.Z;   
        }
        #endregion
        
        _app.GraphicEngine.Objects[Guid.NewGuid()] = _axisObject;
    }

    public void Initialize(World world)
    {
        _world = world;
        _allQuery = Query.For(world)
            .With<ModelComponent>()
            .With<Transform>().Build();
        _selectedQuery = Query.For(world)
            .With<ModelComponent>()
            .With<Transform>()
            .With<SelectedInEditor>().Build();
    }

    public void Update()
    {
        GetCursorVectors(_app.WasdController.View, _app.GraphicEngine.Projection, out Vector3 camPos, out var camDir);
        Update(camPos, camDir);
    }

    
    private void Update(Vector3 camPos, Vector3 camDir)
    {
        if (SelectedEntity is not null) 
            UpdateSelectedEntity(camPos, camDir);
        
        if (_selectedAx != null) return; // don't need to trace anything
        if (!_app.InputHandler.IsPressed(_selectBind)) return;
        if (SelectedEntity is not null)
        {
            // trying to trace axis first
            var mesh = EditorMisc.TraceLineObject(camPos, camDir, _axisModel, _axisObject.TransformMatrix, 5);
            if (mesh != null)
            {
                _selectedAx = _meshToAx[mesh];
                // don't need to trace anything else
                return;
            }
        }

        var traced = TraceObjects(camPos, camDir);
        if (traced is null)
        {
            SelectedEntity = ;
        }
        
        _axisObject.IsHidden = SelectedEntity is null;
    }

    private void ClearSelection()
    {
        _selectedQuery.ForEachArchetype(archetype =>
        {
            for (int row = 0; row < archetype.Rows; row++)
            {
                
            }
        });
    }
    
    private void UpdateSelectedEntity(Vector3 camPos, Vector3 camDir)
    {
        if (SelectedEntity is null) return;
        if (_app.InputHandler.IsJustReleased(_selectBind))
            _selectedAx = null;
        else if (_selectedAx is not null) 
            DragSelectedEntity(camPos, camDir);

        var selectedTransform = _world.GetEntityComponent<Transform>(SelectedEntity.Value);
        if (selectedTransform is null)
        {
            Logger.Warn(this, $"{SelectedEntity.Value} doesn't have {nameof(Transform)}");
            return;
        }

        _axisObject.TransformMatrix = MathUtils.CreateModelTransform(
            selectedTransform.Value.Position, selectedTransform.Value.Rotation, new Vector3(AxisSize));
    }

    private void DragSelectedEntity(Vector3 camPos, Vector3 camDir)
    {
        if (SelectedEntity is null || _selectedAx is null) return;
        if (_app.InputHandler.DeltaCursorPosition == (0, 0)) return;
        
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
            
            var modified = _world.ModifyEntityComponent<Transform>(SelectedEntity.Value, transform =>
            {
                transform.Position = intersection;
                return transform;
            });
            if (!modified) 
                Logger.Warn(this, $"{SelectedEntity.Value} doesn't have {nameof(Transform)}");
            return true;
        }
    }
    
    private Entity? TraceObjects(Vector3 camPos, Vector3 camDir)
    {
        Entity? closest = null;
        var closestDistance = float.MaxValue;
        _allQuery.ForEachArchetype(archetype =>
        {
            var transforms = archetype.GetComponents<Transform>();
            var models = archetype.GetComponents<ModelComponent>();
            for (int row = 0; row < archetype.Rows; row++)
            {
                var model = models[row].Model.Get(out var error);
                if (model is null)
                {
                    Logger.Warn(this, $"Model for {archetype.GetEntityByRow(row)} is null: {error}");
                    continue;
                }
                if (!EditorMisc.TraceObject(camPos, camDir, model, transforms[row].Matrix, out var distance)) continue;
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
        var screenSize = _app.GraphicApi.GetWindowSize();
        Vector2 cursorPos;
        if (_app.GraphicApi.GetCursorMode() == IGraphicApi.CursorMode.Centered) 
            cursorPos = screenSize / 2;
        else
            cursorPos = _app.InputHandler.CursorPosition;

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